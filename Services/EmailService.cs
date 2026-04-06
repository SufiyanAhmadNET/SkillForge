namespace SkillForge.Services;
using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public void SendVerificationEmail(string toEmail, string token, string baseUrl)
    {
        var link = $"{baseUrl}/User/Auth/VerifyEmail?token={token}";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["EmailSettings:SenderName"],
            _config["EmailSettings:SenderEmail"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Verify your SkillForge account";
        message.Body = new TextPart("html")
        {
            Text = $"<h3>Welcome to SkillForge!</h3><p>Click below to verify your email:</p><a href='{link}'>Verify Email</a>"
        };

        using var smtp = new SmtpClient();
        // force IPv4 so MailKit doesn't pick IPv6
        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
        // force IPv4 — IPv6 blocks SMTP on most Indian ISPs
        smtp.ProxyClient = null;
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        // connect using IP directly to force IPv4
        smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        smtp.Authenticate(
            _config["EmailSettings:SenderEmail"],
            _config["EmailSettings:SenderPassword"]);
        smtp.Send(message);
        smtp.Disconnect(true);
    }
}