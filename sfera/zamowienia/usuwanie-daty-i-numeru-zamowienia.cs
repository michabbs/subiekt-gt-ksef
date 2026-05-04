/*
	UWAGA:
	Ten skrypt stał się zbędny bo wprowadzeniu wersji Subiekta 1.88.
	Stosowna opcja konfiguracyjna jest dostępna w programie w standardzie.
*/
/*
	Usuwa z XML FA(3) datę zamówienia i numer zamówienia z sekcji WarunkiTransakcji/Zamowienia.
	To prosty wariant do użycia, gdy te dane nie powinny trafić do finalnej e-Faktury.
*/
dynamic xml = Xml;

try
{
	xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:WarunkiTransakcji/tns:Zamowienia/tns:DataZamowienia");
	xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc/tns:WarunkiTransakcji/tns:Zamowienia/tns:NrZamowienia");
}
catch (Exception ex)
{
	MessageBox.Show(ex.Message);
}
