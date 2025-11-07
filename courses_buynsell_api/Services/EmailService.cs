using courses_buynsell_api.Interfaces;
using System.Net;
using System.Net.Mail;

namespace courses_buynsell_api.Services;

public class EmailService : IEmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _senderPassword;
    private readonly string _senderName;

    public EmailService()
    {
        _smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "smtp.gmail.com";
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        _senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL") ?? "";
        _senderPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD") ?? "";
        _senderName = Environment.GetEnvironmentVariable("SENDER_NAME") ?? "Courses Platform";
    }

    public async Task SendVerificationEmailAsync(string email, string token)
    {
        var encodedToken = System.Web.HttpUtility.UrlEncode(token);
        var verifyUrl = $"http://localhost:5230/api/auth/verify-email?token={encodedToken}";
        var body = $"Click here to verify: <a href='{verifyUrl}'>Verify Email</a>";
        await SendEmailAsync(email, "Email Verification", body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string otp)
    {
        var body = $@"
        <h2>Password Reset Request</h2>
        <p>Your OTP code is: <strong style='font-size: 24px; color: #007bff;'>{otp}</strong></p>
        <p>This code will expire in 15 minutes.</p>
        <p>If you didn't request this, please ignore this email.</p>
    ";
        await SendEmailAsync(email, "Password Reset OTP", body);
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_senderEmail, _senderPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(_senderEmail, _senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await client.SendMailAsync(message);
    }
}