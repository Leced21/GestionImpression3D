using System.Net;
using Backend.Interface;

namespace Backend.Services
{
    public sealed class AuthMailSender : IAuthMailSender
    {
        private readonly IEmailSender _emailSender;

        public AuthMailSender(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public Task SendPasswordResetAsync(string toEmail, string userPrenom, string resetUrl)
        {
            var safeName = WebUtility.HtmlEncode(userPrenom);
            var safeUrl = WebUtility.HtmlEncode(resetUrl);
            var body = $$"""
                <!doctype html>
                <html lang="fr">
                <body style="font-family:Arial,sans-serif;color:#1f2937;line-height:1.6">
                  <h2>Réinitialisation de votre mot de passe</h2>
                  <p>Bonjour {{safeName}},</p>
                  <p>Une demande de réinitialisation de mot de passe a été effectuée pour votre compte 3D Inspire.</p>
                  <p style="margin:28px 0">
                    <a href="{{safeUrl}}" style="background:#0e2841;color:#fff;padding:12px 20px;text-decoration:none;border-radius:6px">Réinitialiser mon mot de passe</a>
                  </p>
                  <p>Ce lien est personnel, valable 30 minutes et utilisable une seule fois.</p>
                  <p>Si vous n'êtes pas à l'origine de cette demande, ignorez cet email : votre mot de passe restera inchangé.</p>
                </body>
                </html>
                """;

            return _emailSender.SendHtmlAsync(toEmail, "Réinitialisation de votre mot de passe — 3D Inspire", body);
        }
    }
}
