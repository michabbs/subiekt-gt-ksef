/*
	Usuwa z XML listę powiązanych dokumentów WZ.
*/
dynamic xml = Xml;
try {
	if (xml.IloscElementow("tns:Faktura/tns:Fa/tns:WZ") > 0)
		xml.UsunElement("tns:Faktura/tns:Fa/tns:WZ");
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
}
