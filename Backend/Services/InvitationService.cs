using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUserRepository _userRepository;

        public InvitationService(IInvitationRepository invitationRepository, IUserRepository userRepository)
        {
            _invitationRepository = invitationRepository;
            _userRepository = userRepository;
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

            return await _invitationRepository.CreateAsync(invitation);
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
