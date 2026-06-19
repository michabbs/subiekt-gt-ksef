/*
	Skrypt dodaje do e-faktury pole "NrZamowienia" i wstawia do niego wartość z podtytułu faktury Subiekta.
	
	Żródło: https://forum.insert.com.pl/index.php?/topic/104818-ksef-numer-zam%C3%B3wienia-na-fakturze-ustrukturyzowanej/#comment-521736
*/
dynamic xml = Xml;

InsERT.SuDokument oDok = null;

try
{
    oDok = (InsERT.SuDokument)Dokument;

    
    string nrZam = Convert.ToString(oDok.Podtytul);
    if (string.IsNullOrWhiteSpace(nrZam))
        return;
    if (xml.IloscElementow("//tns:WarunkiTransakcji") <= 0)
    {
        
        if (xml.IloscElementow("//tns:Zamowienie") > 0)
            xml.DodajElementRownorzednyPrzed("//tns:Zamowienie", "tns:WarunkiTransakcji", "");
        else
            
            xml.DodajElementPodrzedny("//tns:Fa", "tns:WarunkiTransakcji", "");
    }

    
    if (xml.IloscElementow("//tns:WarunkiTransakcji/tns:Zamowienia") <= 0)
    {
        
        if (xml.IloscElementow("//tns:WarunkiTransakcji/tns:NrPartiiTowaru") > 0)
            xml.DodajElementRownorzednyPrzed("//tns:WarunkiTransakcji/tns:NrPartiiTowaru", "tns:Zamowienia", "");
        else if (xml.IloscElementow("//tns:WarunkiTransakcji/tns:WarunkiDostawy") > 0)
            xml.DodajElementRownorzednyPrzed("//tns:WarunkiTransakcji/tns:WarunkiDostawy", "tns:Zamowienia", "");
        else if (xml.IloscElementow("//tns:WarunkiTransakcji/tns:Transport") > 0)
            xml.DodajElementRownorzednyPrzed("//tns:WarunkiTransakcji/tns:Transport", "tns:Zamowienia", "");
        else if (xml.IloscElementow("//tns:WarunkiTransakcji/tns:PodmiotPosredniczacy") > 0)
            xml.DodajElementRownorzednyPrzed("//tns:WarunkiTransakcji/tns:PodmiotPosredniczacy", "tns:Zamowienia", "");
        else
            xml.DodajElementPodrzedny("//tns:WarunkiTransakcji", "tns:Zamowienia", "");
    }

    
    if (xml.IloscElementow("//tns:WarunkiTransakcji/tns:Zamowienia/tns:NrZamowienia") <= 0)
    {
        
        xml.DodajElementPodrzedny("//tns:WarunkiTransakcji/tns:Zamowienia", "tns:NrZamowienia", nrZam);
    }
    else
    {
        
        xml.UstawWartosc("//tns:WarunkiTransakcji/tns:Zamowienia[1]/tns:NrZamowienia", nrZam);
    }
}
catch (Exception ex)
{
    MessageBox.Show(ex.Message);
}
