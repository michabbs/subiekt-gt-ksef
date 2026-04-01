/* 
	Automatyczny wybór rachunku bankowego w zależności od waluty faktury.
	Jeśli do zapłaty jest vat, to oprócz rachunku walutowego na efakturze pojawi się także rachunek złotówkowy
	(bo klient ma prawo zapłacić vat w złotówkach).
	Drugą linijkę powielić dla innych walut (lub usunąć jeśli zbędna) - i uzupełnić listę obsługiwanych walut obcych w ostatniej linijce.
*/
SELECT IBAN='PLxx xxxx xxxx xxxx xxxx xxxx xxxx', SWIFT='XXXXXXXX', NIC=NULL, BANK='Nazwa Baku', OPIS='EUR' WHERE {dok__Dokument.dok_Waluta} = 'EUR'
UNION ALL SELECT 'PLxx xxxx xxxx xxxx xxxx xxxx xxxx', 'XXXXXXXX', NULL, 'Nazwa Baku', 'USD' WHERE {dok__Dokument.dok_Waluta} = 'USD'
UNION ALL SELECT  'PLxx xxxx xxxx xxxx xxxx xxxx xxxx', 'XXXXXXXX', NULL, 'Nazwa Baku', 'PLN'
	WHERE {dok__Dokument.dok_Waluta} NOT IN ('EUR','USD') OR ISNULL({dok__Dokument.dok_WartVat},0)>0
