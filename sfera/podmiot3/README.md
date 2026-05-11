# Uniwersalna obsługa dodatkowych podmiotów w KSeF

Skrypt pozwala dodać do faktury dodatkowe podmioty w polach **Podmiot3** lub **PodmiotUpowazniony** e-faktury.

## Instalacja

 - Dodaj skypt `podmiot3.cs` do Subiekta GT w sposób opisany w dokumentacji Subiekta. (*`Parametry -> KSeF -> Definicje modyfikacji własnych`*)
 - Opcjonalne (ale wskazane!): Jeśli masz którykolwiek z dodatków *Plus dla Subiekta GT* dodaj rozszerzone pole własne **tekstowe** o nazwie **"Podmiot3"** dla wszystkich rodzajów faktur. (*`Parametry -> Pola własne`*)

## Użytkowanie

Do użycia dodatku niezbędne jest posiadanie licencji na *Sferę* dla każdego stanowiska, na którym wystawiane są faktury.

Aby dodać do e-faktury dodatkowy podmiot umieść specjalny znacznik (opisany niżej) w polu własnym **Podmiot3** faktury (zakładka *Własne*). Jeśli nie masz tego pola własnego (bo nie masz *Plusa*) - umieść znacznik w polu **Uwagi** (zakładka *Opis*). Używanie pola **Uwagi** nie jest wskazane, ponieważ często Subiekt jest tak skonfigurowany, aby umieszczać jego zawartość w treści faktury, co nie będzie wyglądać ładnie.

Aby dodać do e-faktury kilka dodatkowych podmiotów - po prostu dodaj kilka znaczników pod rząd.

**Uwaga:** Subiekt GT w standardzie obsługuje już dodawanie pewnych rodzajów podmiotów trzecich (np. *Odbiorca*). Generalnie należy w pierwszej kolejności używać wbudowanych funkcji Subiekta. Ten skrypt pozwala dodać dodatkowe podmioty, które mogą być potrzebne, a na dodanie których Subiekt nie pozwala.

## Format znacznika

    [ROLA:KONTRAHENT|NrKlienta|opcje]

Gdzie:

 - `ROLA` - tekstowy lub numeryczny identyfikator roli kontrahenta (lista niżej)
 - `KONTRAHENT` - symbol kontrahenta. (W tym miejscu dopuszczalne też także wpisanie numeru NIP, ale lepiej podawać symbol, ponieważ ten sam NIP może być powtórzony dla wielu kontrahentów w bazie danych.)
 - `NrKlienta` - pole opcjonalne, można je zostawić puste. Czasem umowa z kontrahentem może wymagać podania na fakturze jakiegoś numeru - KSeF przewiduje na to specjalne pole.
 - `opcje` - pole opcjonalne.  Niektóre role wymagają podania dodatkowych parametrów (opis niżej).

## Wszystkie możliwe role

Obsługiwane są wszystkie role podmiotów, zdefiniowane w dokumentacji KSeF.

Role w sekcji **Podmiot3** e-faktury:
|Kod numeryczny|Kod tekstowy|Rodzaj roli|Zawartość pola `opcje`|
|--|--|--|--|
|1|FAKTOR|Faktor|Numer konta faktora
|2|ODBIORCA|Odbiorca|
|3|PIERWOTNY|Podmiot pierwotny|
|4|NABYWCA|Dodatkowy nabywca|Udział procentowy
|5|WYSTAWCA|Wystawca faktury|
|6|PLATNIK|Dokonujący płatności|
|7|JST-WYSTAWCA|Wystawca JST|
|8|JST|Odbiorca JST|
|9|GV-WYSTAWCA|Wystawca członek GV|
|10|GV|Odbiorca członek GV|
|11|PRACOWNIK|Pracownik|
| |INNA|Inna rola|Opis innej roli

Role w sekcji **PodmiotUpowazniony** e-faktury:
|Kod numeryczny|Kod tekstowy|Rodzaj roli|
|--|--|--|
|U1|OE|Organ egzekucyjny|
|U2|KOMORNIK|Komornik|
|U3|PP|Przedstawiciel podatkowy|

# Przykłady 

## Faktor

Dla faktora w polu `opcje` możesz podać numer konta. Zostanie ono dodane do e-Faktury jako rachunek bankowy faktora, a własny rachunek sprzedawcy zostanie **usuniety**. Jeśli nie podasz numeru konta - zostanie automatycznie pobrany **domyślny rachunek** faktora z bazy Subiekta. Jeśli nie chesz w ogóle umieszczać w e-Fakturze konta faktora - wpisz w to miejsce "`-`".

 - `[FAKTOR:SYMBOL]` - dodaje faktora z domyślnym rachunkiem bankowym, bez numeru klienta.
 - `[FAKTOR:SYMBOL||012345678901234567890123456]` - dodaje faktora z podanym rachunkiem bankowym, bez numeru klienta
 - `[1:SYMBOL|ABC123|-]` - dodaje faktora bez rachunku bankowego, z numerem klienta `ABC123`


## Dodatkowy nabywca

Dla dodatkowego nabywcy w polu `opcje` możesz podać jego udział procentowy. Dodatkowych nabywców może być kilku. Jeśli nie podano udziałów - przyjmuje się, że są one równe. Dla głównego nabywcy (**Podmiot2**) nie podaje się udziału - przypada na niego dopełnienie do 100%.

 - `[NABYWCA:SYMBOL]` - dodaje nabywcę z domyślnym udziałem, bez numeru klienta
 - `[NABYWCA:SYMBOL||19.3]` - dodaje nabywcę z udziałem 19.3%, bez numeru klienta
 - `[4:SYMBOL|qaz123]` - dodaje nabywcę z domyślnym udziałem, z numerem klienta `qaz123`


## Odbiorca / Odbiorca JST / Odbiorca GV

Subiekt GT w standardzie udostępnia już obsługę tych ról, ale pozwala na dodanie tylko **jednej naraz**. Jeśli zachodzi potrzeba dodania więcej niż jednej, albo z jakiegoś powodu nie chcesz używać wbudowanej funkcjonalności, to możesz dodać dodatkowe podmioty tym skryptem.

 - `[ODBIORCA:SYMBOL]` - dodaje odbiorcę, bez numeru klienta
 - `[JST:SYMBOL|12345]` - dodaje odbiorcę JST, z numerem klienta `12345`
 - `[GV:SYMBOL]` - dodaje odbiorcę GV, bez numeru klienta

## Inna rola

 - `[INNY:SYMBOL||Spowiednik]` - dodaje podmiot z inną rolą i opisem `Spowiednik`, bez numeru klienta
 - `[:SYMBOL|666|Kusiciel]` - dodaje podmiot z inną rolą i opisem `Kusiciel`, z numerem klienta `666`


# Gwarancja

**Nie ma żadnej.**
Ten skypt został napisany w celach niekomercyjnych i używasz go na włąsną odpowiedzialność.
