/*
	Przykład: wygenerowanie PDF i wysłanie go mailem w trakcie generowania XML.
	Uwaga: skrypt uruchamia się przed realnym wysłaniem dokumentu do KSeF.
	To efekt uboczny, który może spowolnić lub zablokować proces, więc wymaga ostrożności.
*/
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

InsERT.SuDokument dok = null;

try
{
	dok = (InsERT.SuDokument)Dokument;

	var numer = Convert.ToString(dok.NumerPelny);
	var safeNumer = string.Join("_", numer.Split(Path.GetInvalidFileNameChars()));
	var pdfPath = Path.Combine(Path.GetTempPath(), safeNumer + ".pdf");
	dok.DrukujDoPliku(pdfPath, InsERT.TypPlikuEnum.gtaTypPlikuPDF);

	var mail = new MailMessage();
	mail.From = new MailAddress("twoj_mail@firma.pl");
	mail.To.Add("odbiorca@firma.pl");
	mail.Subject = "Dokument " + numer;
	mail.Body = "W załączeniu PDF dokumentu wygenerowany podczas tworzenia eFaktury.";

	if (File.Exists(pdfPath))
		mail.Attachments.Add(new Attachment(pdfPath));

	var smtp = new SmtpClient("smtp.firma.pl", 587);
	smtp.EnableSsl = true;
	smtp.Credentials = new NetworkCredential("twoj_mail@firma.pl", "haslo");

	smtp.Send(mail);

	MessageBox.Show("PDF wygenerowany i mail wysłany.");
}
catch (Exception ex)
{
	MessageBox.Show(ex.Message);
}
