/* 
	Przenoszenie opisu pozycji z faktury do pola OpisPozycji w efakturze:
*/
SELECT * FROM (
	SELECT
		'Opis' klucz,
		CASE WHEN ob_TowId IS NULL THEN dbo.fnElementUslugiJednorazowej(2, ob_Opis) ELSE ob_Opis END AS wartosc
	FROM dok_Pozycja
	WHERE ob_id = {dok_Pozycja.ob_Id}
) x
WHERE LEN(wartosc)>0
