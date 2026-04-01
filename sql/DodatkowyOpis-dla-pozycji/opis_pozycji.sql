/* 
	Przenoszenie opisu pozycji z faktury do pola OpisPozycji w efakturze:
*/
SELECT  'Opis', ob_Opis
FROM dok_Pozycja
WHERE
	ob_id = {dok_Pozycja.ob_Id}
	AND LEN(ob_Opis)>0
	AND ob_TowId is not null
