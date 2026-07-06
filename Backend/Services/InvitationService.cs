using Backend.Interface;
using Backend.Models;
using System.Net;

namespace Backend.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public InvitationService(
            IInvitationRepository invitationRepository,
            IUserRepository userRepository,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _invitationRepository = invitationRepository;
            _userRepository = userRepository;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task<User> AcceptInvitationAsync(string token, string password, string nom, string prenom)
        {
            var invitation = await _invitationRepository.GetByTokenAsync(token);

            if (invitation == null || invitation.IsUsed || invitation.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Invitation invalide ou expirée");

            var user = new User
            {
                Email = invitation.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Nom = nom,
                Prenom = prenom,
                Role = invitation.Role,
                IsActive = true,
                DateCreation = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);

            invitation.IsUsed = true;
            invitation.AcceptedAt = DateTime.UtcNow;
            await _invitationRepository.UpdateAsync(invitation);

            return createdUser;
        }

        public async Task<bool> CancelInvitationAsync(int id)
        {
            return await _invitationRepository.DeleteAsync(id);
        }

        public async Task<Invitation> CreateInvitationAsync(string email, string role, int invitedBy)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var invitation = new Invitation
            {
                Email = email,
                Token = token,
                Role = role,
                InvitedBy = invitedBy,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            var createdInvitation = await _invitationRepository.CreateAsync(invitation);

            var frontendBaseUrl = (_configuration["Frontend:BaseUrl"] ?? "http://localhost:4200").TrimEnd('/');
            var invitationUrl = $"{frontendBaseUrl}/accept-invitation?token={Uri.EscapeDataString(createdInvitation.Token)}";
            var safeUrl = WebUtility.HtmlEncode(invitationUrl);
            var safeRole = WebUtility.HtmlEncode(createdInvitation.Role);
            var body = $$"""
                <!doctype html>
                <html lang="fr">
                <body style="font-family:Arial,sans-serif;color:#1f2937;line-height:1.6">
                  <h2>Invitation 3D Inspire</h2>
                  <p>Vous avez été invité à rejoindre 3D Inspire avec le rôle <strong>{{safeRole}}</strong>.</p>
                  <p style="margin:28px 0">
                    <a href="{{safeUrl}}" style="background:#3b82f6;color:#fff;padding:12px 20px;text-decoration:none;border-radius:6px">Accepter l'invitation</a>
                  </p>
                  <p>Cette invitation expire dans 7 jours.</p>
                  <p>Si vous n'attendiez pas cette invitation, ignorez cet email.</p>
                </body>
                </html>
                """;

            try
            {
                await _emailSender.SendHtmlAsync(createdInvitation.Email, "Invitation à rejoindre 3D Inspire", body);
            }
            catch
            {
                await _invitationRepository.DeleteAsync(createdInvitation.Id);
                throw;
            }

            return createdInvitation;
        }

        public async Task<List<Invitation>> GetPendingInvitationsAsync()
        {
            var invitations = await _invitationRepository.GetPendingAsync();
            return invitations.ToList();
        }

        public async Task<bool> ValidateInvitationAsync(string token)
        {
            return await _invitationRepository.IsTokenValidAsync(token);
        }
    }
}
