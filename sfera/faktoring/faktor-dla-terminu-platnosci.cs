/*
	Autor: Przemysław Kwiatkowski

	Skrypt automatycznie dodaje faktora do e-faktury. Zastępuje także numer konta sprzedawcy, numerem konta faktora.

	Konfiguracja:
	- Dodać w Subiekcie jeden lub więcej terminów płatności (Słowniki -> Terminy płatności -> Dodaj) o nazwie
	  zaczynającej się od słowa "Faktoring" - np. "Faktoring 30 dni".
	- Wpisać prawidłowe dane teleadresowe i bankowe faktora w treści skryptu poniżej.
	
	Użycie:
	- Wystawić fakturę normalnie.
	- Jako formę płatności wybrać "kredyt kupiecki".
	- W polu "termin płatności" (w lewym dolnym rogu okna edycji faktury) wybrać pozycję zaczynjącą się od słowa "Faktoring".

	Uwaga:
	Skrypt wpływa jedynie zawartość e-faktry, a nie na wygląd wzorca wydruku faktury z Subiekta.
	W celu uzyskania wydruku z danymi faktora należy stosownie dopazować wzorzec wydruku Subiekta,
	albo po prostu wydrukować wizualizację e-faktry wg widoku "MF" i nie uzywać standardowych wydruków Subiekta.
*/
using System.Globalization;

// dane faktora [uzupełnić ręcznie]:
string NIP = "1234567890"; // (musi być 10 cyfr)
string NazwaPelna = "Wielki Faktor Sp. z o.o.";
string AdresL1 = "Malinowa 14/69";
string AdresL2 = "01-234 Pcim Dolny";
string konto="12345678901234567890123456"; // (musi być 26 cyfr)
// koniec danych faktora
// [dalej nie edytować]


dynamic xml = Xml;
InsERT.SuDokument oDok = (InsERT.SuDokument)Dokument;
InsERT.Subiekt sGT = (InsERT.Subiekt)oDok.Aplikacja;
InsERT.Baza oBaza = oDok.Aplikacja.Baza;

decimal? ToDecimal(object value) {
	if (value == null) return null;
	try { return Convert.ToDecimal(value, CultureInfo.InvariantCulture); }
	catch {
		var s = Convert.ToString(value, CultureInfo.InvariantCulture);
		if (string.IsNullOrWhiteSpace(s)) return null;
		if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d1)) return d1;
		if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("pl-PL"), out var d2)) return d2;
		return null;
	}
}

string SQL2string (string sSQL) {
    string s="";
    ADODB.Recordset rs = new ADODB.RecordsetClass();
    rs.Open (sSQL, oBaza.Polaczenie);
	if (!rs.EOF && rs.Fields.Count>0 && rs.Fields[0].Value!=DBNull.Value) s = rs.Fields[0].Value.ToString();
	return s;
}


try {
	var kredytId = ToDecimal(oDok.PlatnoscKredytId);
	string formaPlatnosci=SQL2string("SELECT top 1 fp_Nazwa FROM sl_FormaPlatnosci WHERE fp_Id="+kredytId);

	if (formaPlatnosci.StartsWith("Faktoring")) {
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot2", "tns:Podmiot3", "");
		xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3", "tns:DaneIdentyfikacyjne", "");
	    xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:NIP", NIP);
		xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:Nazwa", NazwaPelna);
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:Adres", "");
		xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:Adres", "tns:KodKraju", "PL");
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:Adres/tns:KodKraju", "tns:AdresL1", AdresL1);
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:Adres/tns:AdresL1", "tns:AdresL2", AdresL2);
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:Adres", "tns:Rola", "1");
		if (xml.IloscElementow("tns:Faktura/tns:Fa/tns:Platnosc/tns:RachunekBankowy") > 0) {
		    xml.DodajElementRownorzednyZa("tns:Faktura/tns:Fa/tns:Platnosc/tns:RachunekBankowy", "tns:RachunekBankowyFaktora", "");
		    xml.DodajElementPodrzedny("tns:Faktura/tns:Fa/tns:Platnosc/tns:RachunekBankowyFaktora", "tns:NrRB", konto);
		    xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:RachunekBankowy");	
		}
	}
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
	throw ex;
}