
/* SenderGrid Email servisi için olan kod bloğudur. Çalışmaktadır
 * Sender Verification'ı olan sender emaili ve api key' i ekleyerek çalıştırabilirsiniz

using OnlineRandevuSistemi.Business.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public class SendGridEmailService : IEmailService
{
    private readonly string _apiKey = "SENDER_GRİD_API_KEY"; 
    public async Task SendAsync(string to, string subject, string body)
    {
       
        Console.WriteLine($"[SendGrid] Mail gönderiliyor: {to}");

        var client = new SendGridClient(_apiKey);

        var fromEmail = "verificated_sender@example.com"; 
        var from = new EmailAddress(fromEmail, "Randevu Sistemi");
        Console.WriteLine("[DEBUG] FROM = " + from.Email);
        Console.WriteLine("[DEBUG] TO = " + to);
        if (string.IsNullOrWhiteSpace(to))
        {
            Console.WriteLine("[SendGrid] ALICI EMAIL BOŞ! Mail gönderilemedi.");
            return;
        }

        var toEmail = new EmailAddress(to);

        var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);

        var response = await client.SendEmailAsync(msg);

        Console.WriteLine($"[SendGrid] StatusCode: {response.StatusCode}");

        if ((int)response.StatusCode >= 400)
        {
            var error = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"[SendGrid] Gönderim başarısız: {error}");
        }
    }
}

*/