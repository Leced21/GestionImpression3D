using System.Net;
using Backend.Interface;

namespace Backend.Services
{
    public sealed class ClientPortalMailSender : IClientPortalMailSender
    {
        private readonly IEmailSender _emailSender;

        public ClientPortalMailSender(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public Task SendMagicLinkAsync(string toEmail, string clientNom, string magicLinkUrl)
        {
            var safeName = WebUtility.HtmlEncode(clientNom);
            var safeUrl = WebUtility.HtmlEncode(magicLinkUrl);
            var body = $$"""
                <!doctype html>
                <html lang="fr">
                <body style="font-family:Arial,sans-serif;color:#1f2937;line-height:1.6">
                  <h2>Accès à votre portail PrintFlow3D</h2>
                  <p>Bonjour {{safeName}},</p>
                  <p>Utilisez le bouton ci-dessous pour accéder à votre espace client.</p>
                  <p style="margin:28px 0">
                    <a href="{{safeUrl}}" style="background:#3b82f6;color:#fff;padding:12px 20px;text-decoration:none;border-radius:6px">Accéder à mon portail</a>
                  </p>
                  <p>Ce lien est personnel, temporaire et utilisable une seule fois.</p>
                  <p>Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.</p>
                </body>
                </html>
                """;

            return _emailSender.SendHtmlAsync(toEmail, "Votre accès au portail PrintFlow3D", body);
        }
    }
}
