/*
	Obsługa faktur za usługi ciągłe - np. czynsz, abonamnet. (Taka faktura powinna zawierać informację o okresie od... do...)
	Dodać do faktury pole własne typu data o nazwie "Początek okresu faktury". (Wymaga któegoś Plusa!)
	Parametry -> Pola własne -> Obiekt: Faktura VAT sprzedaży -> Dodaj pole rozszerzone
	Początek okresu wpisywać w w/w polu własnym.
	Koniec okresu wpisywać w polu "Data zakończenia dostawy".
*/
using System;
using System.Globalization;

dynamic xml = Xml;

DateTime? ToDate(object value) {
	if (value == null) return null;
	try {
		return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
	} catch {
		var s = Convert.ToString(value, CultureInfo.InvariantCulture);
		if (string.IsNullOrWhiteSpace(s)) return null;
		if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1)) return d1;
		if (DateTime.TryParse(s, new CultureInfo("pl-PL"), DateTimeStyles.None, out var d2)) return d2;
		return null;
	}
}

void SetOrInsertValue(dynamic xml, string elementPath, string value, string[] followingSiblingPaths, string[] precedingSiblingPaths) {
	if (xml.UstawWartosc(elementPath, value) > 0) return;

	var lastSlash = elementPath.LastIndexOf('/');
	if (lastSlash <= 0)	return;

	var parentPath = elementPath.Substring(0, lastSlash);
	var elementName = elementPath.Substring(lastSlash + 1);

	if (followingSiblingPaths != null) foreach (var followingSiblingPath in followingSiblingPaths)	{
			if (xml.IloscElementow(followingSiblingPath) > 0) {
				xml.DodajElementRownorzednyPrzed(followingSiblingPath, elementName, value);
				return;
			}
		}

	if (precedingSiblingPaths != null) foreach (var precedingSiblingPath in precedingSiblingPaths)	{
			if (xml.IloscElementow(precedingSiblingPath) > 0) {
				xml.DodajElementRownorzednyZa(precedingSiblingPath, elementName, value);
				return;
			}
		}

	xml.DodajElementPodrzedny(parentPath, elementName, value);
}


try {
	InsERT.SuDokument oDok = (InsERT.SuDokument)Dokument;

	var dataOd = ToDate(oDok.get_PoleWlasne("Początek okresu faktury"));
	var dataDo = ToDate(oDok.DataZakonczeniaDostawy);
	var fakturaOkresowa = dataOd.HasValue && dataDo.HasValue;

	if (fakturaOkresowa) {
		// Usuwamy pole P_6 (data zakończenia dostawy)
		if (xml.IloscElementow("tns:Faktura/tns:Fa/tns:P_6") > 0) xml.UsunElement("tns:Faktura/tns:Fa/tns:P_6");

		// Dodajemy pole OkresFa
		SetOrInsertValue(xml, $"tns:Faktura/tns:Fa/tns:OkresFa", "", 
		new[]
		{
			$"tns:Faktura/tns:Fa/tns:P_13_1",
			$"tns:Faktura/tns:Fa/tns:P_14_1",
			$"tns:Faktura/tns:Fa/tns:P_14_1W",
			$"tns:Faktura/tns:Fa/tns:P_13_2",
			$"tns:Faktura/tns:Fa/tns:P_14_2",
			$"tns:Faktura/tns:Fa/tns:P_14_2W",
			$"tns:Faktura/tns:Fa/tns:P_13_3",
			$"tns:Faktura/tns:Fa/tns:P_14_3",
			$"tns:Faktura/tns:Fa/tns:P_14_3W",
			$"tns:Faktura/tns:Fa/tns:P_13_4",
			$"tns:Faktura/tns:Fa/tns:P_14_4",
			$"tns:Faktura/tns:Fa/tns:P_14_4W",
			$"tns:Faktura/tns:Fa/tns:P_13_5",
			$"tns:Faktura/tns:Fa/tns:P_14_5",
			$"tns:Faktura/tns:Fa/tns:P_13_6_1",
			$"tns:Faktura/tns:Fa/tns:P_13_6_2",
			$"tns:Faktura/tns:Fa/tns:P_13_6_3",
			$"tns:Faktura/tns:Fa/tns:P_13_7",
			$"tns:Faktura/tns:Fa/tns:P_13_8",
			$"tns:Faktura/tns:Fa/tns:P_13_9",
			$"tns:Faktura/tns:Fa/tns:P_13_10",
			$"tns:Faktura/tns:Fa/tns:P_13_11",
			$"tns:Faktura/tns:Fa/tns:P_15",
			$"tns:Faktura/tns:Fa/tns:KursWalutyZ",
			$"tns:Faktura/tns:Fa/tns:Adnotacje",
			$"tns:Faktura/tns:Fa/tns:RodzajFaktury"
		},
		new[]
		{
			$"tns:Faktura/tns:Fa/tns:WZ",
			$"tns:Faktura/tns:Fa/tns:P_2"
		});
		
		xml.DodajElementPodrzedny("tns:Faktura/tns:Fa/tns:OkresFa", "tns:P_6_Od", dataOd.Value.ToString("yyyy-MM-dd"));
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Fa/tns:OkresFa/tns:P_6_Od", "tns:P_6_Do", dataDo.Value.ToString("yyyy-MM-dd"));
	}
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
}