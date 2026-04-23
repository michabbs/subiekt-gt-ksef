/*
	Dodaje do e-Faktury stopkę o stałej treści.
*/
string stopka = "Tu wpisać treść stopki o długości 3500 znaków czystym tekstem. W stopce faktury można zawrzeć np. podziękowanie za zakup, zachętę do dalszej współpracy, kod rabatowy do wykorzystania przy okazji kolejnych zakupów, godziny otwarcia punktu sprzedaży, godziny pracy infolinii/punktu obsługi klienta, link (wyłącznie w formie tekstowej) do formularza zwrotu towaru, link (wyłącznie w formie tekstowej) do formularza reklamacyjnego, informacje marketingowe, klauzulę RODO, wartość kapitału zakładowego itp.";


dynamic xml = Xml;
try {
	if (xml.IloscElementow("tns:Faktura/tns:Stopka/tns:Informacje") < 1)
		if (xml.IloscElementow("tns:Faktura/tns:Stopka/tns:Rejestry") > 0)
			xml.DodajElementRownorzednyPrzed("tns:Faktura/tns:Stopka/tns:Rejestry", "tns:Informacje", "");
		else
			xml.DodajElementPodrzedny("tns:Faktura/tns:Stopka", "tns:Informacje", "");
		xml.DodajElementPodrzedny("tns:Faktura/tns:Stopka/tns:Informacje", "tns:StopkaFaktury", stopka);
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
}
