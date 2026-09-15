using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;

namespace University_Management_System.Services
{
    public class EmailService
    {

        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
    string toEmail,
    string subject,
    string body)
        {
            var email = new MimeKit.MimeMessage();

            email.From.Add(new MailboxAddress(
    _configuration["EmailSettings:SenderName"],
    _configuration["EmailSettings:SenderEmail"]
));

            email.To.Add(MailboxAddress.Parse(toEmail));

            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };


            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:Port"]!),
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
    _configuration["EmailSettings:Username"],
    _configuration["EmailSettings:Password"]
);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}
