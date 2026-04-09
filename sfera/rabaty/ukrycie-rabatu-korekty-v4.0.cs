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

decimal? DeltaValue(decimal? beforeValue, decimal? afterValue)
{
    if (!beforeValue.HasValue || !afterValue.HasValue) return null;
    return afterValue.Value - beforeValue.Value;
}

string FormatDecimal(decimal value)
    => value.ToString("0.########", CultureInfo.InvariantCulture);

void SetValueIfElementExists(dynamic xml, string elementPath, decimal? value)
{
    if (!value.HasValue)
        return;

    if (xml.IloscElementow(elementPath) > 0)
        xml.UstawWartosc(elementPath, FormatDecimal(value.Value));
}

void ApplyDiscountCleanup(dynamic xml, string rowPath, decimal? netPrice, decimal? grossPrice)
{
    if (string.IsNullOrWhiteSpace(rowPath))
        return;

    if (xml.IloscElementow($"{rowPath}/tns:P_10") == 0)
        return;

    if (!(netPrice.HasValue && grossPrice.HasValue))
        return;

    xml.UstawWartosc($"{rowPath}/tns:P_9A", FormatDecimal(netPrice.Value));
    xml.UstawWartosc($"{rowPath}/tns:P_9B", FormatDecimal(grossPrice.Value));
    xml.UsunElement($"{rowPath}/tns:P_10");
}

void ApplyOrderRowValues(dynamic xml, string rowPath, decimal? netPrice, decimal? netTotal, decimal? vatTotal)
{
    if (string.IsNullOrWhiteSpace(rowPath))
        return;

    SetValueIfElementExists(xml, $"{rowPath}/tns:P_9AZ", netPrice);
    SetValueIfElementExists(xml, $"{rowPath}/tns:P_11NettoZ", netTotal);
    SetValueIfElementExists(xml, $"{rowPath}/tns:P_11VatZ", vatTotal);
}

string BuildAllRowsPath(dynamic xml, string basePath, string uuidElementName, string uuid, int positionsCount)
{
    if (!string.IsNullOrWhiteSpace(uuid))
    {
        var rowsPathByUuid = $"{basePath}[tns:{uuidElementName}=\"{uuid}\"]";
        if (xml.IloscElementow(rowsPathByUuid) > 0)
            return rowsPathByUuid;
    }

    if (positionsCount == 1 && xml.IloscElementow(basePath) > 0)
        return basePath;

    return null;
}

void ApplyCorrectionRowSet(
    dynamic xml,
    string allRowsPath,
    string beforeRowsPath,
    string afterRowsPath,
    bool isAddition,
    bool isSingleDeltaValueCorrection,
    Action<string> applyBefore,
    Action<string> applyAfter,
    Action<string> applyDelta)
{
    if (string.IsNullOrWhiteSpace(allRowsPath))
        return;

    var allRows = xml.IloscElementow(allRowsPath);
    var beforeRows = xml.IloscElementow(beforeRowsPath);
    var afterRows = xml.IloscElementow(afterRowsPath);

    if (allRows == 2 && beforeRows == 1 && afterRows == 1)
    {
        applyBefore(beforeRowsPath);
        applyAfter(afterRowsPath);
        return;
    }

    if (allRows == 1 && beforeRows == 1)
    {
        applyBefore(beforeRowsPath);
        return;
    }

    if (allRows == 1 && afterRows == 1)
    {
        if (isAddition)
        {
            applyAfter(afterRowsPath);
            return;
        }

        if (isSingleDeltaValueCorrection)
            applyDelta(afterRowsPath);
    }
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
        var orderRowBasePath = "tns:Faktura/tns:Fa/tns:Zamowienie/tns:ZamowienieWiersz";

        for (int i = 1; i <= positions.Liczba; ++i)
        {
            position = (InsERT.SuPozycjaKorekty)positions.Wczytaj(i);

            try
            {
                var uuid = Convert.ToString(position.KsefUUID);

                var beforeQuantity = ToDecimal(position.IloscJm) ?? ToDecimal(position.Ilosc);
                var afterQuantity = ToDecimal(position.IloscJmPoKorekcie) ?? ToDecimal(position.IloscPoKorekcie);

                var beforeNetTotal = ToDecimal(position.WartoscNettoPoRabacie);
                var beforeVatTotal = ToDecimal(position.WartoscVatPoRabacie);
                var beforeGrossTotal = ToDecimal(position.WartoscBruttoPoRabacie);
                var afterNetTotal = ToDecimal(position.WartoscNettoPoRabaciePoKorekcie);
                var afterVatTotal = ToDecimal(position.WartoscVatPoRabaciePoKorekcie);
                var afterGrossTotal = ToDecimal(position.WartoscBruttoPoRabaciePoKorekcie);

                var beforeNetPrice = UnitPrice(ToDecimal(position.CenaNettoPoRabacie), beforeNetTotal, beforeQuantity);
                var beforeGrossPrice = UnitPrice(ToDecimal(position.CenaBruttoPoRabacie), beforeGrossTotal, beforeQuantity);
                var afterNetPrice = UnitPrice(ToDecimal(position.CenaNettoPoRabaciePoKorekcie), afterNetTotal, afterQuantity);
                var afterGrossPrice = UnitPrice(ToDecimal(position.CenaBruttoPoRabaciePoKorekcie), afterGrossTotal, afterQuantity);

                var deltaNetPrice = DeltaUnitPrice(beforeNetTotal, afterNetTotal, beforeQuantity, afterQuantity);
                var deltaGrossPrice = DeltaUnitPrice(beforeGrossTotal, afterGrossTotal, beforeQuantity, afterQuantity);
                var deltaNetTotal = DeltaValue(beforeNetTotal, afterNetTotal);
                var deltaVatTotal = DeltaValue(beforeVatTotal, afterVatTotal);

                var isAddition =
                    IsZeroish(beforeQuantity) &&
                    IsZeroish(beforeNetTotal) &&
                    IsZeroish(beforeGrossTotal) &&
                    (!IsZeroish(afterQuantity) || !IsZeroish(afterNetTotal) || !IsZeroish(afterGrossTotal));

                var isSingleDeltaValueCorrection =
                    (!IsZeroish(beforeQuantity) || !IsZeroish(afterQuantity)) &&
                    SameDecimal(beforeQuantity, afterQuantity) &&
                    (!SameDecimal(beforeNetTotal, afterNetTotal) ||
                     !SameDecimal(beforeVatTotal, afterVatTotal) ||
                     !SameDecimal(beforeGrossTotal, afterGrossTotal));

                var faAllRowsPath = BuildAllRowsPath(xml, documentRowPath, "UU_ID", uuid, positions.Liczba);
                var faBeforeRowsPath = string.IsNullOrWhiteSpace(faAllRowsPath) ? null : $"{faAllRowsPath}[tns:StanPrzed=\"1\"]";
                var faAfterRowsPath = string.IsNullOrWhiteSpace(faAllRowsPath) ? null : $"{faAllRowsPath}[not(tns:StanPrzed=\"1\")]";

                ApplyCorrectionRowSet(
                    xml,
                    faAllRowsPath,
                    faBeforeRowsPath,
                    faAfterRowsPath,
                    isAddition,
                    isSingleDeltaValueCorrection,
                    rowPath => ApplyDiscountCleanup(xml, rowPath, beforeNetPrice, beforeGrossPrice),
                    rowPath => ApplyDiscountCleanup(xml, rowPath, afterNetPrice, afterGrossPrice),
                    rowPath => ApplyDiscountCleanup(xml, rowPath, deltaNetPrice, deltaGrossPrice));

                var orderAllRowsPath = BuildAllRowsPath(xml, orderRowBasePath, "UU_IDZ", uuid, positions.Liczba);
                var orderBeforeRowsPath = string.IsNullOrWhiteSpace(orderAllRowsPath) ? null : $"{orderAllRowsPath}[tns:StanPrzedZ=\"1\"]";
                var orderAfterRowsPath = string.IsNullOrWhiteSpace(orderAllRowsPath) ? null : $"{orderAllRowsPath}[not(tns:StanPrzedZ=\"1\")]";

                ApplyCorrectionRowSet(
                    xml,
                    orderAllRowsPath,
                    orderBeforeRowsPath,
                    orderAfterRowsPath,
                    isAddition,
                    isSingleDeltaValueCorrection,
                    rowPath => ApplyOrderRowValues(xml, rowPath, beforeNetPrice, beforeNetTotal, beforeVatTotal),
                    rowPath => ApplyOrderRowValues(xml, rowPath, afterNetPrice, afterNetTotal, afterVatTotal),
                    rowPath => ApplyOrderRowValues(xml, rowPath, deltaNetPrice, deltaNetTotal, deltaVatTotal));
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
