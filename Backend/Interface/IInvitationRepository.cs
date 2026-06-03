using Backend.Models;

namespace Backend.Interface
{
    public interface IInvitationRepository
    {
        Task<Invitation?> GetByIdAsync(int id);
        Task<Invitation?> GetByTokenAsync(string token);
        Task<IEnumerable<Invitation>> GetPendingAsync();
        Task<Invitation> CreateAsync(Invitation invitation);
        Task<Invitation> UpdateAsync(Invitation invitation);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsTokenValidAsync(string token);
    }
}
