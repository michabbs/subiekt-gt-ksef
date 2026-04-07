/*
	Usuwa z XML kod GTIN - do użycia jeśli nie powinien trafić do finalnej e-Faktury.
	Co do zasady - kod GTIN jest pobierany z pola "podstawowy kod kreskowy" i jest to znakomity sposób
	wiązania towarów, wiec nie powinien być usuwany. Jeśli ktoś jednak stosuje swoje wewnętrzne kody,
	to ich usunięcie z faktur wychodzących "na zewnątrz" może być celowe.
*/
dynamic xml = Xml;
try {
	if (xml.IloscElementow("tns:Faktura/tns:Fa/tns:FaWiersz") > 0)
		xml.UsunElement("tns:Faktura/tns:Fa/tns:FaWiersz/tns:GTIN");
	if (xml.IloscElementow("tns:Faktura/tns:Fa/tns:Zamowienie/tns:ZamowienieWiersz") > 0)
		xml.UsunElement("tns:Faktura/tns:Fa/tns:Zamowienie/tns:ZamowienieWiersz/tns:GTINZ");
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
}
