/*
	Autor: Krzysztof Wielgosz
	Pochodzenie: wątek forum Insert "E-faktura KSEF i wysyłka za pobraniem - jak oznaczyć?"

	Prostszy wariant usuwania sekcji Platnosc.
	Pozostawia sekcję tylko wtedy, gdy w XML istnieje TerminPlatnosci.
	Przydaje się jako lekki modyfikator, gdy nie jest potrzebna bardziej rozbudowana logika.
	UWAGA: Usuwa także numer rachunku bankowego!
*/
using System;

dynamic xml = Xml;

try {
	var maTerminPlatnosci = xml.IloscElementow("tns:Faktura/tns:Fa/tns:Platnosc/tns:TerminPlatnosci") > 0;

	if (!maTerminPlatnosci && xml.IloscElementow("tns:Faktura/tns:Fa/tns:Platnosc") > 0) {
		xml.UsunElement("tns:Faktura/tns:Fa/tns:Platnosc");
	}
} catch (Exception ex) {
	MessageBox.Show(ex.Message);
}
