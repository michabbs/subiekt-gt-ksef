## Skrypty sferyczne do modyfikacji e-Faktur

Zamieszczone tu modyfikacjie wymagają licencji na Sferę dla każdego stanowiska, na którym wystawiane są faktury.

Instlacja modyfikacji:
Parametry -> KSeF -> Dane e-Faktur -> Definicje modyfikacji własnych

Przeczytać informacje zawarte w pomocy progamu! (F1)

Dostępne przykłady:
- [formy_platnosci/usuwanie-sekcji-platnosci-bez-terminu.cs](formy_platnosci/usuwanie-sekcji-platnosci-bez-terminu.cs) - prosty wariant usuwania sekcji płatności bez terminu.
- [formy_platnosci/usuwanie-zaplat-czastkowych.cs](formy_platnosci/usuwanie-zaplat-czastkowych.cs) - bezpieczniejszy wariant dla płatności częściowych i kredytu kupieckiego.
- [faktury_okresowe/faktury_okresowe.cs](faktury_okresowe/faktury_okresowe.cs) - obsługa okresu rozliczeniowego w e-Fakturze.
- [powiadomienia_mailowe/wysylanie-maila-przy-generowaniu-xml.cs](powiadomienia_mailowe/wysylanie-maila-przy-generowaniu-xml.cs) - wysyłka prostego maila podczas generowania XML.
- [powiadomienia_mailowe/generowanie-pdf-i-wysylka-mail.cs](powiadomienia_mailowe/generowanie-pdf-i-wysylka-mail.cs) - generowanie PDF i wysyłka maila przed wysłaniem do KSeF.
- [diagnostyka/dostep-do-com-dokumentu-podczas-generowania-xml.cs](diagnostyka/dostep-do-com-dokumentu-podczas-generowania-xml.cs) - pokazuje dostęp do obiektu COM dokumentu i pozycji.
- [gtin/usuwanie-gtin.cs](gtin/usuwanie-gtin.cs) - usuwa kod GTIN z e-Faktury
- [stopka/dodaj-stopke.cs](stopka/dodaj-stopke.cs) - dodaje do e-Faktury stopkę o stałej treści

## Informacje techniczne

Dla chcących samodzielnie modyfikować skrypty - listę dostępnych metod zawiera [ten post](https://forum.insert.com.pl/index.php?/topic/108382-modyfikacje-w%C5%82asne-ksef-odczyt-warto%C5%9Bci-z-xml-i-lista-dost%C4%99pnych-metod-obiektu-xml/&do=findComment&comment=543298) na forum.
