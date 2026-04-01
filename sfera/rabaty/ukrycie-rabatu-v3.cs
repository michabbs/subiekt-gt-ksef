/*
	Ukrywa rabat w zwykłych fakturach FA(3), podstawiając wartości po rabacie do odpowiednich pól XML.
	Skrypt działa na modelu COM dokumentu i synchronizuje także sekcję ZamowienieWiersz, jeśli występuje.
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

decimal? UnitPrice(decimal? directPrice, decimal? totalValue, decimal? quantity)
{
	if (directPrice.HasValue) return directPrice;
	if (totalValue.HasValue && quantity.HasValue && quantity.Value != 0m)
		return Math.Round(totalValue.Value / quantity.Value, 8, MidpointRounding.AwayFromZero);

	return null;
}

string FormatDecimal(decimal value)
	=> value.ToString("0.########", CultureInfo.InvariantCulture);

void SetOrInsertValue(dynamic xml, string elementPath, string value, string[] followingSiblingPaths, string[] precedingSiblingPaths)
{
	if (xml.UstawWartosc(elementPath, value) > 0)
		return;

	var lastSlash = elementPath.LastIndexOf('/');
	if (lastSlash <= 0)
		return;

	var parentPath = elementPath.Substring(0, lastSlash);
	var elementName = elementPath.Substring(lastSlash + 1);

	if (followingSiblingPaths != null)
	{
		foreach (var followingSiblingPath in followingSiblingPaths)
		{
			if (xml.IloscElementow(followingSiblingPath) > 0)
			{
				xml.DodajElementRownorzednyPrzed(followingSiblingPath, elementName, value);
				return;
			}
		}
	}

	if (precedingSiblingPaths != null)
	{
		foreach (var precedingSiblingPath in precedingSiblingPaths)
		{
			if (xml.IloscElementow(precedingSiblingPath) > 0)
			{
				xml.DodajElementRownorzednyZa(precedingSiblingPath, elementName, value);
				return;
			}
		}
	}

	xml.DodajElementPodrzedny(parentPath, elementName, value);
}

void ApplyFaDiscountCleanup(dynamic xml, string rowPath, decimal? netPrice, decimal? grossPrice)
{
	if (string.IsNullOrWhiteSpace(rowPath))
		return;

	if (xml.IloscElementow($"{rowPath}/tns:P_10") == 0)
		return;

	if (!(netPrice.HasValue && grossPrice.HasValue))
		return;

	SetOrInsertValue(
		xml,
		$"{rowPath}/tns:P_9A",
		FormatDecimal(netPrice.Value),
		new[]
		{
			$"{rowPath}/tns:P_9B",
			$"{rowPath}/tns:P_10",
			$"{rowPath}/tns:P_11",
			$"{rowPath}/tns:P_11A",
			$"{rowPath}/tns:P_12",
			$"{rowPath}/tns:P_12_XII"
		},
		new[]
		{
			$"{rowPath}/tns:P_8B",
			$"{rowPath}/tns:P_8A"
		});

	SetOrInsertValue(
		xml,
		$"{rowPath}/tns:P_9B",
		FormatDecimal(grossPrice.Value),
		new[]
		{
			$"{rowPath}/tns:P_10",
			$"{rowPath}/tns:P_11",
			$"{rowPath}/tns:P_11A",
			$"{rowPath}/tns:P_12",
			$"{rowPath}/tns:P_12_XII"
		},
		new[]
		{
			$"{rowPath}/tns:P_9A",
			$"{rowPath}/tns:P_8B",
			$"{rowPath}/tns:P_8A"
		});

	// Usuwamy rabat dopiero po podstawieniu obu cen po rabacie.
	xml.UsunElement($"{rowPath}/tns:P_10");
}

void ApplyOrderRowValues(dynamic xml, string rowPath, decimal? netPrice, decimal? netTotal, decimal? vatTotal)
{
	if (string.IsNullOrWhiteSpace(rowPath))
		return;

	if (!(netPrice.HasValue && netTotal.HasValue && vatTotal.HasValue))
		return;

	SetOrInsertValue(
		xml,
		$"{rowPath}/tns:P_9AZ",
		FormatDecimal(netPrice.Value),
		new[]
		{
			$"{rowPath}/tns:P_11NettoZ",
			$"{rowPath}/tns:P_11VatZ",
			$"{rowPath}/tns:P_12Z",
			$"{rowPath}/tns:P_12Z_XII",
			$"{rowPath}/tns:P_12Z_Zal_15",
			$"{rowPath}/tns:GTUZ",
			$"{rowPath}/tns:ProceduraZ",
			$"{rowPath}/tns:KwotaAkcyzyZ",
			$"{rowPath}/tns:StanPrzedZ"
		},
		new[]
		{
			$"{rowPath}/tns:P_8BZ",
			$"{rowPath}/tns:P_8AZ"
		});

	SetOrInsertValue(
		xml,
		$"{rowPath}/tns:P_11NettoZ",
		FormatDecimal(netTotal.Value),
		new[]
		{
			$"{rowPath}/tns:P_11VatZ",
			$"{rowPath}/tns:P_12Z",
			$"{rowPath}/tns:P_12Z_XII",
			$"{rowPath}/tns:P_12Z_Zal_15",
			$"{rowPath}/tns:GTUZ",
			$"{rowPath}/tns:ProceduraZ",
			$"{rowPath}/tns:KwotaAkcyzyZ",
			$"{rowPath}/tns:StanPrzedZ"
		},
		new[]
		{
			$"{rowPath}/tns:P_9AZ",
			$"{rowPath}/tns:P_8BZ",
			$"{rowPath}/tns:P_8AZ"
		});

	SetOrInsertValue(
		xml,
		$"{rowPath}/tns:P_11VatZ",
		FormatDecimal(vatTotal.Value),
		new[]
		{
			$"{rowPath}/tns:P_12Z",
			$"{rowPath}/tns:P_12Z_XII",
			$"{rowPath}/tns:P_12Z_Zal_15",
			$"{rowPath}/tns:GTUZ",
			$"{rowPath}/tns:ProceduraZ",
			$"{rowPath}/tns:KwotaAkcyzyZ",
			$"{rowPath}/tns:StanPrzedZ"
		},
		new[]
		{
			$"{rowPath}/tns:P_11NettoZ",
			$"{rowPath}/tns:P_9AZ",
			$"{rowPath}/tns:P_8BZ",
			$"{rowPath}/tns:P_8AZ"
		});
}

string ResolveUniqueRowPath(dynamic xml, string basePath, string uuidElementName, string uuid, int positionsCount)
{
	if (!string.IsNullOrWhiteSpace(uuid))
	{
		var rowPathByUuid = $"{basePath}[tns:{uuidElementName}=\"{uuid}\"]";
		if (xml.IloscElementow(rowPathByUuid) == 1)
			return rowPathByUuid;
	}

	if (positionsCount == 1 && xml.IloscElementow(basePath) == 1)
		return basePath;

	return null;
}

dynamic xml = Xml;
InsERT.SuDokument oDok = null;
InsERT.SuPozycje oPozycje = null;
InsERT.SuPozycja oPoz = null;

try
{
	oDok = (InsERT.SuDokument)Dokument;

	// Korekty rozpoznajemy po typie dokumentu z API Sfery.
	var czyKorekta =
		oDok.Typ == (int)InsERT.SuDokumentTypEnum.gtaSuDokumentTypKFS ||
		oDok.Typ == (int)InsERT.SuDokumentTypEnum.gtaSuDokumentTypKFM;

	if (!czyKorekta)
	{
		oPozycje = (InsERT.SuPozycje)oDok.Pozycje;

		for (int i = 1; i <= oPozycje.Liczba; ++i)
		{
			oPoz = (InsERT.SuPozycja)oPozycje.Wczytaj(i);

			try
			{
				var uuid = Convert.ToString(oPoz.KsefUUID);
				var ilosc = ToDecimal(oPoz.IloscJm) ?? ToDecimal(oPoz.Ilosc);

				var wartoscNettoPoRabacie = ToDecimal(oPoz.WartoscNettoPoRabacie);
				var wartoscVatPoRabacie = ToDecimal(oPoz.WartoscVatPoRabacie);
				var wartoscBruttoPoRabacie = ToDecimal(oPoz.WartoscBruttoPoRabacie);

				var cenaNettoPoRabacie = UnitPrice(ToDecimal(oPoz.CenaNettoPoRabacie), wartoscNettoPoRabacie, ilosc);
				var cenaBruttoPoRabacie = UnitPrice(ToDecimal(oPoz.CenaBruttoPoRabacie), wartoscBruttoPoRabacie, ilosc);

				var sciezkaFaWiersz = ResolveUniqueRowPath(
					xml,
					"tns:Faktura/tns:Fa/tns:FaWiersz",
					"UU_ID",
					uuid,
					oPozycje.Liczba);

				var sciezkaZamowienieWiersz = ResolveUniqueRowPath(
					xml,
					"tns:Faktura/tns:Fa/tns:Zamowienie/tns:ZamowienieWiersz",
					"UU_IDZ",
					uuid,
					oPozycje.Liczba);

				ApplyFaDiscountCleanup(xml, sciezkaFaWiersz, cenaNettoPoRabacie, cenaBruttoPoRabacie);
				ApplyOrderRowValues(xml, sciezkaZamowienieWiersz, cenaNettoPoRabacie, wartoscNettoPoRabacie, wartoscVatPoRabacie);
			}
			finally
			{
				if (oPoz != null)
				{
					Marshal.ReleaseComObject(oPoz);
					oPoz = null;
				}
			}
		}
	}
}
finally
{
	if (oPozycje != null) Marshal.ReleaseComObject(oPozycje);
}
