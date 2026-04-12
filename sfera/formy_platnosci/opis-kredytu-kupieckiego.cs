/*
	DLa faktur w kredycie kupieckim dodaje opis formy płatności z Subiekta (np. "Odroczony 14 dni")
*/
using System;
using System.Globalization;

dynamic xml = Xml;

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


InsERT.SuDokument oDok = (InsERT.SuDokument)Dokument;
var kredytId = ToDecimal(oDok.PlatnoscKredytId);

if (kredytId.HasValue) try {
	InsERT.Baza oBaza = oDok.Aplikacja.Baza;
	String sSQL = "SELECT top 1 * FROM sl_FormaPlatnosci WHERE fp_Id="+kredytId;

	ADODB.Recordset rs = new ADODB.RecordsetClass();
	String formaPlatnosci="";

	rs.Open (sSQL, oBaza.Polaczenie);
	if(!rs.EOF) {
		formaPlatnosci=(String)rs.Fields["fp_Nazwa"].Value;
		rs.MoveNext();
	}

	if ( formaPlatnosci!="" && xml.IloscElementow("tns:Faktura/tns:Fa/tns:Platnosc/tns:TerminPlatnosci")>0 && xml.IloscElementow("tns:Faktura/tns:Fa/tns:Platnosc/tns:PlatnoscInna")==0 ){
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Fa/tns:Platnosc/tns:FormaPlatnosci", "tns:PlatnoscInna", "1");
		xml.DodajElementRownorzednyZa("tns:Faktura/tns:Fa/tns:Platnosc/tns:PlatnoscInna", "tns:OpisPlatnosci", formaPlatnosci);
		xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:FormaPlatnosci");
	}
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
}
