using Backend.Interface;

namespace Backend.Services
{
    // Utilisé quand aucun SMTP n'est configuré (Mail:Smtp:Host vide) : on logge le contenu
    // de l'email au lieu de l'envoyer, pour ne pas bloquer le développement/les tests locaux.
    // À ne jamais utiliser en production : ce stub expose le contenu de l'email (liens
    // d'invitation/d'accès inclus) en clair dans les logs serveur.
    public sealed class LoggingEmailSender : IEmailSender
    {
        private readonly ILogger<LoggingEmailSender> _logger;

        public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendHtmlAsync(string toEmail, string subject, string htmlBody)
        {
            _logger.LogInformation(
                "[STUB EMAIL] À {Recipient} — {Subject}\n{Body}",
                toEmail, subject, htmlBody);

            return Task.CompletedTask;
        }
    }
}
