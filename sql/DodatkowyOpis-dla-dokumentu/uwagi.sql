/* 
	Przenosi pole "Uwagi" z dokumentu do dodatkowego opisu e-Faktury.
*/
SELECT nic, klucz, tekst FROM (
	SELECT 
		NULL as nic,
		'Uwagi' as klucz,
		LEFT(TRIM(REPLACE(dok_Uwagi, CHAR(13)+CHAR(10), ' ')), 256) as tekst
	FROM dok__Dokument
	WHERE dok_Id={dok__Dokument.dok_Id}

	UNION ALL SELECT
		NULL,
		'Uwagi',
		SUBSTRING(TRIM(REPLACE(dok_Uwagi, CHAR(13)+CHAR(10), ' ')), 257, 256)
	FROM dok__Dokument
	WHERE dok_Id={dok__Dokument.dok_Id}
) x 
WHERE LEN(tekst)>0
