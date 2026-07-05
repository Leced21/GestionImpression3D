using Backend.Interface;

namespace Backend.Services
{
    // Aucun fournisseur d'email n'est branché pour l'instant (pas de SMTP/SendGrid configuré
    // dans ce projet). En attendant, on se contente de logger le lien pour ne pas bloquer le
    // développement/les tests du portail client. À remplacer par un vrai envoi d'email avant
    // toute mise en production : ce stub expose le lien magique en clair dans les logs serveur.
    public class LoggingClientPortalMailSender : IClientPortalMailSender
    {
        private readonly ILogger<LoggingClientPortalMailSender> _logger;
        public LoggingClientPortalMailSender(ILogger<LoggingClientPortalMailSender> logger)
        {
            _logger = logger;
        }

        public Task SendMagicLinkAsync(string toEmail, string clientNom, string magicLinkUrl)
        {
            _logger.LogInformation(
                "[STUB EMAIL] Lien d'accès portail client pour {ClientNom} <{Email}> : {MagicLinkUrl}",
                clientNom, toEmail, magicLinkUrl);

            return Task.CompletedTask;
        }
    }
}
