using System.Net;
using System.Net.Mail;

namespace ShareIT.Services
{

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailAsync(string[] to, string[] cc, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // SMTP settings come from configuration (appsettings + user-secrets),
            // never hardcoded. See README for local setup.
            var host = _configuration["Email:Host"] ?? "smtp.office365.com";
            var port = int.TryParse(_configuration["Email:Port"], out var p) ? p : 587;
            var user = _configuration["Email:User"];
            var password = _configuration["Email:Password"];
            var from = _configuration["Email:From"] ?? user;

            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            _logger.LogInformation($"Sending email to {to}: {subject}");

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {to}");
        }
    }

    public async Task SendEmailAsync(string[] to, string[] cc, string subject, string body)
        {
            try
            {
                foreach (var recipient in to)
                {
                    await SendEmailAsync(recipient, subject, body);
                }
                foreach (var recipient in cc)
                {
                    await SendEmailAsync(recipient, subject, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk emails");
            }
        }
    }
}