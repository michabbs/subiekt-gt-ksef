Skrypty i dodatki ułatwiające konfigurację KSEF w systemie Subiekt GT.

Wątek na forum Insert o skryptach i modyfikatorach KSeF:
https://forum.insert.com.pl/index.php?/topic/105225-e-faktura-ksef-i-wysy%C5%82ka-za-pobraniem-jak-oznaczy%C4%87/page/3/#comment-543219

## Struktura repo

### Plik główny

- `README.md` - opis repo oraz mapa katalogów i plików.

### Katalog `sql`

Skrypty SQL do definicji modyfikacji własnych w KSeF po stronie Subiekta GT.

- `sql/RachunekBankowy/automatyczny_rachunek_walutowy.sql` - wybiera rachunek bankowy zależnie od waluty faktury; dla VAT pozwala równolegle pokazać rachunek złotówkowy.
- `sql/Indeks/symbol_tylko_dla_towarow.sql` - uzupełnia pole indeks tylko dla towarów i kompletów, pozostawia je puste dla usług.
- `sql/DodatkowyOpis-dla-pozycji/opis_pozycji.sql` - przenosi opis pozycji z dokumentu Subiekta do pola `OpisPozycji` w e-Fakturze.
- `sql/DodatkowyOpis-dla-dokumentu/dodatkowy_opis.sql` - dodaje stały, dodatkowy opis do całej e-Faktury; można go rozszerzać o kolejne linie `UNION ALL`.

### Katalog `sfera`

Modyfikacje w C# wymagające licencji Sfera na stanowiskach wystawiających faktury.

- `sfera/README.md` - krótka instrukcja instalacji modyfikacji Sfera: `Parametry -> KSeF -> Dane e-Faktur -> Definicje modyfikacji własnych`.
- `sfera/formy_platnosci/usuwanie-sekcji-platnosci-bez-terminu.cs` - prosty modyfikator z forum Insert autorstwa Krzysztofa Wielgosza; usuwa całą sekcję `Platnosc`, jeśli w XML nie ma `TerminPlatnosci`.
- `sfera/formy_platnosci/usuwanie-zaplat-czastkowych.cs` - usuwa z XML e-Faktury informacje o zapłatach częściowych i czyści sekcję płatności dla płatności natychmiastowych; zostawia dane rachunku oraz termin dla realnego kredytu kupieckiego z terminem > 0 dni.
- `sfera/faktury_okresowe/faktury_okresowe.cs` - obsługuje faktury za usługi ciągłe; zamienia standardowe `P_6` na sekcję `OkresFa` na podstawie pola własnego `Początek okresu faktury` i daty zakończenia dostawy.
- `sfera/powiadomienia_mailowe/wysylanie-maila-przy-generowaniu-xml.cs` - wysyła prosty mail w trakcie generowania XML; przykład techniczny pokazujący, że w hooku działa zwykły kod .NET.
- `sfera/powiadomienia_mailowe/generowanie-pdf-i-wysylka-mail.cs` - generuje PDF dokumentu i wysyła go mailem jeszcze przed realną wysyłką do KSeF.
- `sfera/diagnostyka/dostep-do-com-dokumentu-podczas-generowania-xml.cs` - pokazuje, że modyfikator ma dostęp nie tylko do XML, ale też do COM dokumentu i pozycji Sfery.
- `sfera/zamowienia/usuwanie-daty-i-numeru-zamowienia.cs` - usuwa z FA(3) datę zamówienia i numer zamówienia z sekcji `WarunkiTransakcji/Zamowienia`.
- `sfera/rabaty/ukrycie-rabatu-v3.cs` - ukrywa rabat w zwykłych fakturach, podstawiając do XML wartości po rabacie.
- `sfera/rabaty/ukrycie-rabatu-korekty-v3.cs` - ukrywa rabat w korektach, obsługując warianty przed/po oraz wiersze zamówienia.

## Szybki wybór

- Rachunek bankowy zależny od waluty: `sql/RachunekBankowy/automatyczny_rachunek_walutowy.sql`
- Indeks tylko dla towarów: `sql/Indeks/symbol_tylko_dla_towarow.sql`
- Opis pozycji w XML KSeF: `sql/DodatkowyOpis-dla-pozycji/opis_pozycji.sql`
- Dodatkowy opis całego dokumentu: `sql/DodatkowyOpis-dla-dokumentu/dodatkowy_opis.sql`
- Proste usunięcie całej sekcji płatności bez terminu: `sfera/formy_platnosci/usuwanie-sekcji-platnosci-bez-terminu.cs`
- Czyszczenie informacji o płatnościach częściowych: `sfera/formy_platnosci/usuwanie-zaplat-czastkowych.cs`
- Obsługa faktur okresowych: `sfera/faktury_okresowe/faktury_okresowe.cs`
- Mail przy generowaniu XML: `sfera/powiadomienia_mailowe/wysylanie-maila-przy-generowaniu-xml.cs`
- PDF i mail przy generowaniu XML: `sfera/powiadomienia_mailowe/generowanie-pdf-i-wysylka-mail.cs`
- Diagnostyka COM dokumentu podczas generowania XML: `sfera/diagnostyka/dostep-do-com-dokumentu-podczas-generowania-xml.cs`
- Usuwanie daty i numeru zamówienia: `sfera/zamowienia/usuwanie-daty-i-numeru-zamowienia.cs`
- Ukrycie rabatu w zwykłych fakturach: `sfera/rabaty/ukrycie-rabatu-v3.cs`
- Ukrycie rabatu w korektach: `sfera/rabaty/ukrycie-rabatu-korekty-v3.cs`
