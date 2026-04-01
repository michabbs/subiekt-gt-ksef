/*
	Usuwamy informacje o dokonanych zapłatach cząstkowych (pobrania, kredyty, przedpłaty, gotówka, przelew).
	Usuwamy informacje o formie płatności jeśli jest to "kredyt kupiecki 0 dni".
	Pozostaje bez zmian informacja o formie płatności jeśli jest to "kredyt kupiecki >0 dni".
	Pozostaje bez znian informacja o rachunku bankowym do dokonania zapłaty.
*/
using System;
using System.Globalization;

dynamic xml = Xml;

decimal? ToDecimal(object value) {
	if (value==null) return null;
	try {
		return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
	} catch {
		var s = Convert.ToString(value, CultureInfo.InvariantCulture);
		if (string.IsNullOrWhiteSpace(s)) return null;
		if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d1)) return d1;
		if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("pl-PL"), out var d2)) return d2;
		return null;
	}
}

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
	if (xml.IloscElementow("tns:Faktura/tns:Fa/tns:Platnosc") > 0) {
		// usuwamy informacje o dokonanych zapłatach cząstkowych
		xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:Zaplacono");
		xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:DataZaplaty");
		xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:ZaplataCzesciowa");
		xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:ZnacznikZaplatyCzesciowej");

		InsERT.SuDokument oDok = (InsERT.SuDokument)Dokument;
		var kwotaKredytu = ToDecimal(oDok.PlatnoscKredytKwota);
		var terminKredytu = ToDate(oDok.PlatnoscKredytTermin);
		var dataWystawienia = ToDate(oDok.DataWystawienia);
		var maOdroczonyTermin = kwotaKredytu.HasValue && (kwotaKredytu.Value>0m) && terminKredytu.HasValue && dataWystawienia.HasValue && terminKredytu.Value.Date > dataWystawienia.Value.Date;

		// usuwamy formę i termin tylko wtedy, gdy nie ma realnego terminu > 0 dni
		if (!maOdroczonyTermin) {
			xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:FormaPlatnosci");
			xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:TerminPlatnosci");
		}

		// usuwamy całą sekcję tylko wtedy, gdy nie zostały już żadne elementy podrzędne
		if (xml.IloscElementow("tns:Faktura/tns:Fa/tns:Platnosc/*") == 0)
			xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc");
	}
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
}
