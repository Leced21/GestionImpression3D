using Backend.Models;

namespace Backend.Interface
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(int id);
        Task<Client?> GetByEmailAsync(string email);
        Task<IEnumerable<Client>> GetAllAsync();
        Task<IEnumerable<Client>> SearchAsync(string searchTerm);
        Task<Client> CreateAsync(Client client);
        Task<Client> UpdateAsync(Client client);
        Task<bool> DeleteAsync(int id);
        Task<int> GetCountAsync();
    }
}
