using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Diagnostics;
// appsettings.json dosyasını yükle
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Email ayarlarını oku
var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();




// E-posta gönderimi
var emailSent = await SendEmailAsync(
    emailSettings.SMTPServer,
    emailSettings.Port,
    emailSettings.SenderEmail,
    emailSettings.Password,
    "muratoksuz208@gmail.com",
    "Test Konusu",
    "Bu bir test e-postasıdır!"
);

Console.WriteLine(emailSent ? "E-posta başarıyla gönderildi!" : "E-posta gönderimi başarısız oldu.");
async Task<bool> SendEmailAsync(
    string smtpServer,
    int port,
    string senderEmail,
    string password,
    string toEmail,
    string subject,
    string body)
{
    try
    {
        var stopwatch = Stopwatch.StartNew();

        using (var smtpClient = new SmtpClient())
        {
            smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true; // Sertifika doğrulamasını geçici olarak devre dışı bırak
            smtpClient.Timeout = 2000; // Maksimum 10 saniye bekleme

            // SMTP sunucusuna bağlanma ve süre ölçümü
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // 10 saniye timeout
            await smtpClient.ConnectAsync(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls, cancellationTokenSource.Token);

            stopwatch.Stop();
            Console.WriteLine($"SMTP bağlantı süresi: {stopwatch.ElapsedMilliseconds} ms");

            // SMTP kimlik doğrulama
            await smtpClient.AuthenticateAsync(senderEmail, password);

            // E-posta oluşturma ve gönderme
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Sistem Adı", senderEmail));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            email.Body = new TextPart("plain")
            {
                Text = body
            };

            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);
        }

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"E-posta gönderimi hatası: {ex.Message}");
        return false;
    }
}


// EmailSettings sınıfı
public class EmailSettings
{
    public string SMTPServer { get; set; }
    public int Port { get; set; }
    public string SenderEmail { get; set; }
    public string Password { get; set; }
}
