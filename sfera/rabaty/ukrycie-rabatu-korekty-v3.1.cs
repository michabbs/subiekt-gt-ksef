/*
	Ukrywa rabat w korektach FA(3), obsługując warianty przed/po oraz wiersze zamówienia.
	Skrypt bazuje na danych COM pozycji korekty i aktualizuje XML tak, by zachować wartości po rabacie.
*/
using System;
using System.Globalization;
using System.Runtime.InteropServices;

decimal? ToDecimal(object value)
{
    if (value == null) return null;
    try { return Convert.ToDecimal(value, CultureInfo.InvariantCulture); }
    catch
    {
        var s = Convert.ToString(value, CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d1)) return d1;
        if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("pl-PL"), out var d2)) return d2;
        return null;
    }
}

bool IsZeroish(decimal? value)
    => !value.HasValue || value.Value == 0m;

bool SameDecimal(decimal? left, decimal? right)
{
    if (!left.HasValue && !right.HasValue) return true;
    if (!left.HasValue || !right.HasValue) return false;
    return decimal.Round(left.Value - right.Value, 6) == 0m;
}

decimal? UnitPrice(decimal? directPrice, decimal? totalValue, decimal? quantity)
{
    if (directPrice.HasValue) return directPrice;
    if (totalValue.HasValue && quantity.HasValue && quantity.Value != 0m)
        return Math.Round(totalValue.Value / quantity.Value, 8, MidpointRounding.AwayFromZero);

    return null;
}

decimal? DeltaUnitPrice(decimal? beforeTotal, decimal? afterTotal, decimal? beforeQuantity, decimal? afterQuantity)
{
    if (!beforeTotal.HasValue || !afterTotal.HasValue) return null;
    if (!beforeQuantity.HasValue || !afterQuantity.HasValue) return null;
    if (!SameDecimal(beforeQuantity, afterQuantity)) return null;
    if (afterQuantity.Value == 0m) return null;

    return Math.Round((afterTotal.Value - beforeTotal.Value) / afterQuantity.Value, 8, MidpointRounding.AwayFromZero);
}

string FormatDecimal(decimal value)
    => value.ToString("0.########", CultureInfo.InvariantCulture);

void ApplyDiscountCleanup(dynamic xml, string rowPath, decimal? netPrice, decimal? grossPrice)
{
    if (xml.IloscElementow($"{rowPath}/tns:P_10") == 0)
        return;

    if (!(netPrice.HasValue && grossPrice.HasValue))
        return;

    xml.UstawWartosc($"{rowPath}/tns:P_9A", FormatDecimal(netPrice.Value));
    xml.UstawWartosc($"{rowPath}/tns:P_9B", FormatDecimal(grossPrice.Value));
    xml.UsunElement($"{rowPath}/tns:P_10");
}

dynamic xml = Xml;
InsERT.SuDokument document = null;
InsERT.SuPozycje positions = null;
InsERT.SuPozycjaKorekty position = null;

try
{
    document = (InsERT.SuDokument)Dokument;

    var isCorrection =
        document.Typ == (int)InsERT.SuDokumentTypEnum.gtaSuDokumentTypKFS ||
        document.Typ == (int)InsERT.SuDokumentTypEnum.gtaSuDokumentTypKFM;

    if (isCorrection)
    {
        positions = (InsERT.SuPozycje)document.Pozycje;
        var documentRowPath = "tns:Faktura/tns:Fa/tns:FaWiersz";

        for (int i = 1; i <= positions.Liczba; ++i)
        {
            position = (InsERT.SuPozycjaKorekty)positions.Wczytaj(i);

            try
            {
                var uuid = Convert.ToString(position.KsefUUID);

                var beforeQuantity = ToDecimal(position.IloscJm) ?? ToDecimal(position.Ilosc);
                var afterQuantity = ToDecimal(position.IloscJmPoKorekcie) ?? ToDecimal(position.IloscPoKorekcie);

                var beforeNetTotal = ToDecimal(position.WartoscNettoPoRabacie);
                var beforeGrossTotal = ToDecimal(position.WartoscBruttoPoRabacie);
                var afterNetTotal = ToDecimal(position.WartoscNettoPoRabaciePoKorekcie);
                var afterGrossTotal = ToDecimal(position.WartoscBruttoPoRabaciePoKorekcie);

                var beforeNetPrice = UnitPrice(ToDecimal(position.CenaNettoPoRabacie), beforeNetTotal, beforeQuantity);
                var beforeGrossPrice = UnitPrice(ToDecimal(position.CenaBruttoPoRabacie), beforeGrossTotal, beforeQuantity);
                var afterNetPrice = UnitPrice(ToDecimal(position.CenaNettoPoRabaciePoKorekcie), afterNetTotal, afterQuantity);
                var afterGrossPrice = UnitPrice(ToDecimal(position.CenaBruttoPoRabaciePoKorekcie), afterGrossTotal, afterQuantity);

                var deltaNetPrice = DeltaUnitPrice(beforeNetTotal, afterNetTotal, beforeQuantity, afterQuantity);
                var deltaGrossPrice = DeltaUnitPrice(beforeGrossTotal, afterGrossTotal, beforeQuantity, afterQuantity);

                var isAddition =
                    IsZeroish(beforeQuantity) &&
                    IsZeroish(beforeNetTotal) &&
                    IsZeroish(beforeGrossTotal) &&
                    (!IsZeroish(afterQuantity) || !IsZeroish(afterNetTotal) || !IsZeroish(afterGrossTotal));

                var isDeletion =
                    (!IsZeroish(beforeQuantity) || !IsZeroish(beforeNetTotal) || !IsZeroish(beforeGrossTotal)) &&
                    IsZeroish(afterQuantity) &&
                    IsZeroish(afterNetTotal) &&
                    IsZeroish(afterGrossTotal);

                var isSingleDeltaValueCorrection =
                    (!IsZeroish(beforeQuantity) || !IsZeroish(afterQuantity)) &&
                    SameDecimal(beforeQuantity, afterQuantity) &&
                    (!SameDecimal(beforeNetTotal, afterNetTotal) || !SameDecimal(beforeGrossTotal, afterGrossTotal));

                string allRowsPath;
                string beforeRowsPath;
                string afterRowsPath;

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    allRowsPath = $"{documentRowPath}[tns:UU_ID=\"{uuid}\"]";
                    beforeRowsPath = $"{allRowsPath}[tns:StanPrzed=\"1\"]";
                    afterRowsPath = $"{allRowsPath}[not(tns:StanPrzed=\"1\")]";
                }
                else if (positions.Liczba == 1)
                {
                    allRowsPath = documentRowPath;
                    beforeRowsPath = $"{documentRowPath}[tns:StanPrzed=\"1\"]";
                    afterRowsPath = $"{documentRowPath}[not(tns:StanPrzed=\"1\")]";
                }
                else
                {
                    continue;
                }

                var allRows = xml.IloscElementow(allRowsPath);
                var beforeRows = xml.IloscElementow(beforeRowsPath);
                var afterRows = xml.IloscElementow(afterRowsPath);

                if (allRows == 2 && beforeRows == 1 && afterRows == 1)
                {
                    ApplyDiscountCleanup(xml, beforeRowsPath, beforeNetPrice, beforeGrossPrice);
                    ApplyDiscountCleanup(xml, afterRowsPath, afterNetPrice, afterGrossPrice);
                    continue;
                }

                if (allRows == 1 && beforeRows == 1)
                {
                    ApplyDiscountCleanup(xml, beforeRowsPath, beforeNetPrice, beforeGrossPrice);
                    continue;
                }

                if (allRows == 1 && afterRows == 1)
                {
                    if (isAddition)
                    {
                        ApplyDiscountCleanup(xml, afterRowsPath, afterNetPrice, afterGrossPrice);
                        continue;
                    }

                    if (isSingleDeltaValueCorrection)
                    {
                        ApplyDiscountCleanup(xml, afterRowsPath, deltaNetPrice, deltaGrossPrice);
                        continue;
                    }
                }
            }
            finally
            {
                if (position != null)
                {
                    Marshal.ReleaseComObject(position);
                    position = null;
                }
            }
        }
    }
}
finally
{
    if (positions != null) Marshal.ReleaseComObject(positions);
}

