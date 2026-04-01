/*
	Przykład diagnostyczny pokazujący, że modyfikator XML działa także w kontekście COM dokumentu.
	Uwaga: to demonstracja możliwości, a nie docelowy modyfikator produkcyjny.
*/
using System;
using System.Runtime.InteropServices;

InsERT.SuDokument dok = null;
InsERT.SuPozycje pozycje = null;
InsERT.SuPozycja poz = null;

try
{
	dok = (InsERT.SuDokument)Dokument;
	pozycje = (InsERT.SuPozycje)dok.Pozycje;

	string txt = "To nie jest tylko XML.\n\n";
	txt += "Numer dokumentu: " + Convert.ToString(dok.NumerPelny) + "\n";
	txt += "Typ dokumentu: " + Convert.ToString(dok.Typ) + "\n";
	txt += "Data wystawienia: " + Convert.ToString(dok.DataWystawienia) + "\n";
	txt += "Liczba pozycji: " + Convert.ToString(pozycje.Liczba) + "\n";

	if (pozycje.Liczba > 0)
	{
		poz = (InsERT.SuPozycja)pozycje.Wczytaj(1);

		txt += "\nPierwsza pozycja:\n";
		txt += "Nazwa: " + Convert.ToString(poz.TowarNazwa) + "\n";
		txt += "Ilość: " + Convert.ToString(poz.IloscJm) + "\n";
		txt += "Cena netto po rabacie: " + Convert.ToString(poz.CenaNettoPoRabacie) + "\n";
		txt += "UUID KSeF pozycji: " + Convert.ToString(poz.KsefUUID) + "\n";
	}

	MessageBox.Show(txt, "Diagnostyka z COM podczas generowania eFaktury");
}
catch (Exception ex)
{
	MessageBox.Show(ex.Message);
}
finally
{
	if (poz != null) Marshal.ReleaseComObject(poz);
	if (pozycje != null) Marshal.ReleaseComObject(pozycje);
}
