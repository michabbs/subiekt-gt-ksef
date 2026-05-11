/*
	Autor: Przemysław Kwiatkowski

	Skrypt pozwala dodać do faktury dodatkowe podmioty w polach Podmiot3 lub PodmiotUpowazniony e-faktury.
	Instrukcja: https://github.com/michabbs/subiekt-gt-ksef/blob/master/sfera/podmiot3/README.md
*/
using System;
using System.Globalization;
using System.Text.RegularExpressions;


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


void DodajDaneBazowe(InsERT.Kontrahent oKh) {
    string NIP = ((String)oKh.NIP).Trim().Replace("-", "");
    string IDWew=SQL2string("SELECT adr_IdWewPodmiot3KSeF FROM adr__Ewid WHERE adr_TypAdresu=1 AND adr_IdObiektu="+(int)oKh.Identyfikator);
	string NrEORI = SQL2string("SELECT adr_NrEORI FROM adr__Ewid WHERE adr_TypAdresu=1 AND adr_IdObiektu="+(int)oKh.Identyfikator);
	string NazwaPelna = (String)oKh.NazwaPelna;
	string KodKrajuUE=SQL2string("SELECT pa_KodPanstwaUE FROM sl_Panstwo WHERE pa_Id="+(int)oKh.Panstwo);
    string KodKraju=SQL2string("SELECT pa_KodPanstwaISO FROM sl_Panstwo WHERE pa_Id="+(int)oKh.Panstwo);
	bool czyUE=(bool)oKh.PodatnikVatUE && KodKrajuUE.Length>0;
    if (NIP.StartsWith(KodKrajuUE)) NIP = NIP.Substring(KodKrajuUE.Length).Trim();
	if (NIP.StartsWith(KodKraju)) NIP = NIP.Substring(KodKraju.Length).Trim();
	string AdresL1 = (string)oKh.Ulica+" "+(string)oKh.NrDomu;
	string NrLokalu = (string)oKh.NrLokalu;
	if (NrLokalu.Length>0) AdresL1 += "/"+NrLokalu;
	string AdresL2 = (string)oKh.KodPocztowy+" "+(string)oKh.Miejscowosc;
	
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot2", "tns:Podmiot3", "");
    if (NrEORI.Length>0) xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3", "tns:NrEORI", NrEORI);
	xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3", "tns:DaneIdentyfikacyjne", "");
    if (IDWew.Length>0) {
        xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:IDWew", IDWew);
    } else if (NIP.Length>0) 
        if (czyUE) {
		    xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:KodUE", KodKrajuUE);
		    xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:NrVatUE", NIP);
        } else if (KodKraju!="PL") {
		    xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:KodKraju", KodKraju);
		    xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:NrID", NIP);
        } else
		    xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:NIP", NIP);
	else
		xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:BrakID", "1");
	xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:Nazwa", NazwaPelna);
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:DaneIdentyfikacyjne", "tns:Adres", "");
	xml.DodajElementPodrzedny("tns:Faktura/tns:Podmiot3/tns:Adres", "tns:KodKraju", KodKraju);
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:Adres/tns:KodKraju", "tns:AdresL1", AdresL1);
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:Adres/tns:AdresL1", "tns:AdresL2", AdresL2);
}

void DodajKontoFaktora(InsERT.Kontrahent oKh, string konto) {
    if ((konto=="-") || (xml.IloscElementow("tns:Faktura/tns:Fa/tns:Platnosc/tns:RachunekBankowy")==0)) return;
    if (konto.Length==0) konto = SQL2string("SELECT TOP 1 rb_Numer FROM rb__RachBankowy WHERE rb_TypObiektu=1 AND rb_Podstawowy=1 AND rb_IdObiektu="+(int)oKh.Identyfikator);
    if (konto.Length==0) return;
    xml.DodajElementRownorzednyZa("tns:Faktura/tns:Fa/tns:Platnosc/tns:RachunekBankowy", "tns:RachunekBankowyFaktora", "");
    xml.DodajElementPodrzedny("tns:Faktura/tns:Fa/tns:Platnosc/tns:RachunekBankowyFaktora", "tns:NrRB", konto);
    xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:RachunekBankowy");
}

decimal reszta_udzialu=100;
void DodajPodmiot3(string s, int rola) {
    string[] dane = s.Split('|');
    string symbol=dane[0];
    string NrKlienta=""; if (dane.Length>1) NrKlienta=dane[1];
    InsERT.Kontrahent oKh;
    try { oKh = (InsERT.Kontrahent)sGT.Kontrahenci.Wczytaj(symbol); }
    catch (Exception ex) {throw new Exception("Nie znaleziono kontrahenta \""+symbol+"\"!");}
    DodajDaneBazowe(oKh);
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:Adres", "tns:Rola", ""+rola);
    if (NrKlienta.Length>0) xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:Rola", "tns:NrKlienta", NrKlienta);

    switch (rola) {
        case 1:     DodajKontoFaktora(oKh, (dane.Length>2)?dane[2]:""); break;
        case 4:     // dodatowy nabywca:
                    decimal? u=0;
                    if (dane.Length>2) u=(dane[2].Length==0)?0:ToDecimal(dane[2]);
                    if (u is null || u.Value<0 || u.Value>100) throw new Exception("Nieprawidłowy udział: \""+dane[2]+"\"");
                    reszta_udzialu -= u.Value;
                    if (reszta_udzialu<0) throw new Exception("Suma udziałów większa od 100%!");
                    if (u.Value>0) xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:Rola", "tns:Udzial", u.Value.ToString(CultureInfo.InvariantCulture));
                    break;
        case 8:     xml.UstawWartosc("tns:Faktura/tns:Podmiot2/tns:JST", "1"); break;
        case 10:    xml.UstawWartosc("tns:Faktura/tns:Podmiot2/tns:GV", "1"); break;
    }
}

void DodajInny(string s) {
    string[] dane = s.Split('|');
    string symbol=dane[0];
    string? rola=null;      if (dane.Length>1) rola=dane[1];
	string NrKlienta=""; if (dane.Length>2) NrKlienta=dane[2];
	if (rola is null) throw new Exception("Nieprawidłowy format danych \""+s+"\"!");
    
    InsERT.Kontrahent oKh;
    try { oKh = (InsERT.Kontrahent)sGT.Kontrahenci.Wczytaj(symbol); }
    catch (Exception ex) {throw new Exception("Nie znaleziono kontrahenta \""+symbol+"\"!");}

    DodajDaneBazowe(oKh);
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:Adres", "tns:RolaInna", "1");
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:RolaInna", "tns:OpisRoli", rola);
    if (NrKlienta.Length>0) xml.DodajElementRownorzednyZa("tns:Faktura/tns:Podmiot3/tns:OpisRoli", "tns:NrKlienta", NrKlienta);
}


void DodajDaneBazowePU(InsERT.Kontrahent oKh) {
    string NIP = ((String)oKh.NIP).Trim().Replace("-", "");
    if (NIP.Length>=2 && char.IsLetter(NIP[0]) && char.IsLetter(NIP[1])) NIP = NIP.Substring(2).Trim();
   	string NrEORI = SQL2string("SELECT adr_NrEORI FROM adr__Ewid WHERE adr_TypAdresu=1 AND adr_IdObiektu="+(int)oKh.Identyfikator);
	string NazwaPelna = (String)oKh.NazwaPelna;
	string KodKraju=SQL2string("SELECT pa_KodPanstwaISO FROM sl_Panstwo WHERE pa_Id="+(int)oKh.Panstwo);
	string AdresL1 = (string)oKh.Ulica+" "+(string)oKh.NrDomu;
	string NrLokalu = (string)oKh.NrLokalu;
	if (NrLokalu.Length>0) AdresL1 += "/"+NrLokalu;
	string AdresL2 = (string)oKh.KodPocztowy+" "+(string)oKh.Miejscowosc;
	
	xml.DodajElementRownorzednyPrzed("tns:Faktura/tns:Fa", "tns:PodmiotUpowazniony", "");
    if (NrEORI.Length>0) xml.DodajElementPodrzedny("tns:Faktura/tns:PodmiotUpowazniony", "tns:NrEORI", NrEORI);
	xml.DodajElementPodrzedny("tns:Faktura/tns:PodmiotUpowazniony", "tns:DaneIdentyfikacyjne", "");
	xml.DodajElementPodrzedny("tns:Faktura/tns:PodmiotUpowazniony/tns:DaneIdentyfikacyjne", "tns:NIP", NIP);
	xml.DodajElementPodrzedny("tns:Faktura/tns:PodmiotUpowazniony/tns:DaneIdentyfikacyjne", "tns:Nazwa", NazwaPelna);
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:PodmiotUpowazniony/tns:DaneIdentyfikacyjne", "tns:Adres", "");
	xml.DodajElementPodrzedny("tns:Faktura/tns:PodmiotUpowazniony/tns:Adres", "tns:KodKraju", KodKraju);
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:PodmiotUpowazniony/tns:Adres/tns:KodKraju", "tns:AdresL1", AdresL1);
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:PodmiotUpowazniony/tns:Adres/tns:AdresL1", "tns:AdresL2", AdresL2);
}

void DodajPodmiotUpowazniony(string symbol, int rola) {
    if (xml.IloscElementow("tns:Faktura/tns:PodmiotUpowazniony") > 0) throw new Exception("Nie dozwolony więcej niż 1 PodmiotUpowazniony!");
    InsERT.Kontrahent oKh;
    try { oKh = (InsERT.Kontrahent)sGT.Kontrahenci.Wczytaj(symbol); }
    catch (Exception ex) {throw new Exception("Nie znaleziono kontrahenta \""+symbol+"\"!");}
    DodajDaneBazowePU(oKh);
	xml.DodajElementRownorzednyZa("tns:Faktura/tns:PodmiotUpowazniony/tns:Adres", "tns:RolaPU", ""+rola);
}

KeyValuePair<string, string>[] ParsujDane(string s) {
    var results = new List<KeyValuePair<string, string>>();

    // dopasowanie: [znacznik:wartość]
    var matches = Regex.Matches(s, @"\[(.*?)(?::(.*?))?\]");
    for (int i = matches.Count - 1; i >= 0; i--) {
        var match = matches[i];
        var key = match.Groups[1].Value.ToUpperInvariant();
        // jeśli nie było ":", wartość = ""
        var value = match.Groups[2].Success ? match.Groups[2].Value : "";
        results.Add(new KeyValuePair<string, string>(key, value));
    }
    return results.ToArray();
}

string PodajDane() {
	string s="";
	try { s=(string)oDok.get_PoleWlasne("Podmiot3"); }
    catch (Exception ex) { s=(string)oDok.Uwagi; }
    return s;
}

try {
    foreach (var para in ParsujDane(PodajDane())) {
        switch (para.Key) {
            case "1": case "FAKTOR":
                DodajPodmiot3(para.Value, 1);    break;
            case "2": case "ODBIORCA":
                DodajPodmiot3(para.Value, 2);    break;
            case "3": case "PIERWOTNY": case "PODMIOT_PIERWOTNY": case "PODMIOTPIERWOTNY": case "PPIERWOTNY":
                DodajPodmiot3(para.Value, 3);    break;
            case "4": case "NABYWCA": case "KLIENT":
                DodajPodmiot3(para.Value, 4);    break;
            case "5": case "WYSTAWCA":
                DodajPodmiot3(para.Value, 5);    break;
            case "6": case "PLATNIK": case "PŁATNIK":
                DodajPodmiot3(para.Value, 6);    break;
            case "7": case "JST-WYSTAWCA":
                DodajPodmiot3(para.Value, 7);    break;
            case "8": case "JST-ODBIORCA": case "JST":
                DodajPodmiot3(para.Value, 8);    break;
            case "9": case "GV-WYSTAWCA":
                DodajPodmiot3(para.Value, 9);    break;
            case "10": case "GV-ODBIORCA": case "GV":
                DodajPodmiot3(para.Value, 10);   break;
            case "11": case "PRACOWNIK":
                DodajPodmiot3(para.Value, 11);   break;
            case "U1": case "OE": case "ORGAN_EGZEKUCYJNY": case "ORGANEGZEKUCYJNY": 
                DodajPodmiotUpowazniony(para.Value, 1);   break;
            case "U2": case "KOMORNIK":
                DodajPodmiotUpowazniony(para.Value, 2);   break;
            case "U3": case "PP": case "PRZEDSTAWICIEL_PODATKOWY": case "PRZEDSTAWICIELPODATKOWY":
                DodajPodmiotUpowazniony(para.Value, 3);   break;
            case "": case "INNY": case "INNA": case "INNE":
                DodajInny(para.Value);           break;
            //default:
            //    throw new Exception("Nieprawidłowy format danych ["+para.Key+"]!");
        }
    }
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
	throw ex;
}
