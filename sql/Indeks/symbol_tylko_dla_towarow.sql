/* 
	W pole indeks wpisujemy symbol towaru (lub kompletu), ale pozostawiamy puste dla usług:
*/
SELECT tw_Symbol
FROM tw__Towar
WHERE
	tw_Id = {dok_Pozycja.ob_TowId}
	AND tw_Rodzaj in (1,8)
