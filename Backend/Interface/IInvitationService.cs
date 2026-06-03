using Backend.Models;

namespace Backend.Interface
{
    public interface IInvitationService
    {
        Task<Invitation> CreateInvitationAsync(string email, string role, int invitedBy);
        Task<bool> ValidateInvitationAsync(string token);
        Task<User> AcceptInvitationAsync(string token, string password, string nom, string prenom);
        Task<List<Invitation>> GetPendingInvitationsAsync();
        Task<bool> CancelInvitationAsync(int id);
    }
}
