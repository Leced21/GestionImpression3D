using System.Net;
using System.Net.Mail;
using Backend.Interface;
using Backend.Options;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendHtmlAsync(string toEmail, string subject, string htmlBody)
        {
            ValidateConfiguration();

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = Math.Max(1, _options.TimeoutSeconds) * 1000
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Email SMTP envoyé à {Recipient} avec le sujet {Subject}", toEmail, subject);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Échec de l'envoi SMTP à {Recipient}", toEmail);
                throw new InvalidOperationException("L'envoi de l'email a échoué. Vérifiez la configuration SMTP.", ex);
            }
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
                throw new InvalidOperationException("La configuration Mail:Smtp:Host est manquante.");
            if (_options.Port is < 1 or > 65535)
                throw new InvalidOperationException("La configuration Mail:Smtp:Port est invalide.");
            if (string.IsNullOrWhiteSpace(_options.FromEmail))
                throw new InvalidOperationException("La configuration Mail:Smtp:FromEmail est manquante.");
        }
    }
}
