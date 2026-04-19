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

    public void SendOtpEmail(string toEmail, string otp)
    {
        var message = new MimeMessage();

        var senderName = _config["EmailSettings:SenderName"] ?? string.Empty;
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? string.Empty;
        var senderPassword = _config["EmailSettings:SenderPassword"] ?? string.Empty;

        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Your SkillForge OTP Code";

        message.Body = new TextPart("html")
        {
            Text = $@"
            <div style='font-family:Arial;padding:20px;text-align:center;'>
                <h2>Email Verification</h2>
                <p>Your OTP is:</p>
                <div style='font-size:28px;font-weight:bold;letter-spacing:8px;
                            padding:10px;background:#f1f1f1;border-radius:8px;'>
                    {otp}
                </div>
                <p>This OTP is valid for 5 minutes.</p>
            </div>"
        };

        using var smtp = new SmtpClient();

        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

        smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        smtp.Authenticate(senderEmail, senderPassword);
        smtp.Send(message);
        smtp.Disconnect(true);
    }
}