/*
	Przykład: wysłanie maila w trakcie generowania XML.
	Uwaga: skrypt uruchamia się przed realnym wysłaniem dokumentu do KSeF.
	To efekt uboczny, który w środowisku produkcyjnym może blokować proces generowania XML.
*/
using System;
using System.Net;
using System.Net.Mail;

try
{
	var dok = (InsERT.SuDokument)Dokument;

	var numer = Convert.ToString(dok.NumerPelny);
	var data = Convert.ToString(dok.DataWystawienia);

	var mail = new MailMessage();
	mail.From = new MailAddress("twoj_mail@firma.pl");
	mail.To.Add("odbiorca@firma.pl");
	mail.Subject = "Generowanie eFaktury";
	mail.Body =
		"Właśnie generowany jest XML dla dokumentu:\n\n" +
		"Numer: " + numer + "\n" +
		"Data: " + data;

	var smtp = new SmtpClient("smtp.firma.pl", 587);
	smtp.EnableSsl = true;
	smtp.Credentials = new NetworkCredential("twoj_mail@firma.pl", "haslo");

	smtp.Send(mail);

	MessageBox.Show("Mail wysłany.");
}
catch (Exception ex)
{
	MessageBox.Show(ex.Message);
}
