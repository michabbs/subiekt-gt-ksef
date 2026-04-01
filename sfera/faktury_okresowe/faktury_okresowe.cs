/*
	Obsługa faktur za usługi ciągłe - np. czynsz, abonamnet. (Taka faktura powinna zawierać informację o okresie od... do...)
	Dodać do faktury pole własne typu data o nazwie "Początek okresu faktury". (Wymaga któegoś Plusa!)
	Parametry -> Pola własne) -> Obiekt: Faktura VAT sprzedaży -> Dodaj pole rozrzeżone
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

try {
	InsERT.SuDokument oDok = (InsERT.SuDokument)Dokument;

	var dataOd = ToDate(oDok.get_PoleWlasne("Początek okresu faktury"));
	var dataDo = ToDate(oDok.DataZakonczeniaDostawy);
	var fakturaOkresowa = dataOd.HasValue && dataDo.HasValue;

	if (fakturaOkresowa) {
		// Usuwamy pole P_6 (data zakończenia dostawy)
		if (xml.IloscElementow("tns:Faktura/tns:Fa/tns:P_6") > 0) xml.UsunElement("tns:Faktura/tns:Fa/tns:P_6");

		// Dodajemy pole OkresFa
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Fa/tns:P_2", "tns:OkresFa", "");
		xml.DodajElementPodrzedny("tns:Faktura/tns:Fa/tns:OkresFa", "tns:P_6_Od", dataOd.Value.ToString("yyyy-MM-dd"));
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Fa/tns:OkresFa/tns:P_6_Od", "tns:P_6_Do", dataDo.Value.ToString("yyyy-MM-dd"));
	}
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
}
