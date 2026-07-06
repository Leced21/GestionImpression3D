using Backend.Models;

namespace Backend.Interface
{
    public interface IClientMagicLinkRepository
    {
        Task<ClientMagicLink> CreateAsync(ClientMagicLink link);
        Task<ClientMagicLink?> GetByTokenHashAsync(string tokenHash);
        Task<ClientMagicLink> UpdateAsync(ClientMagicLink link);
    }
}
