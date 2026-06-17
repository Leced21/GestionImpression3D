using Backend.DTOs;
using Backend.Models;

namespace Backend.Interface
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task<Client?> GetByEmailAsync(string email);
        Task<IEnumerable<Client>> SearchAsync(string searchTerm);
        Task<Client> CreateAsync(CreateClientRequest request);
        Task<Client?> UpdateAsync(int id, UpdateClientRequest request);
        Task<bool> DeleteAsync(int id);
        Task<int> GetCountAsync();
    }
}
