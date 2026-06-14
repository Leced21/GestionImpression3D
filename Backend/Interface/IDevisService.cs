using Backend.DTOs;
using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IDevisService
    {
        Task<IEnumerable<Devis>> GetAllAsync();
        Task<Devis?> GetByIdAsync(int id);
        Task<IEnumerable<Devis>> GetByClientAsync(int clientId);
        Task<Devis> CreateAsync(CreateDevisRequest request);
        Task<Devis?> UpdateStatutAsync(int id, DevisStatus statut);
        Task<byte[]> GeneratePdfAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<DevisStatisticsDto> GetStatisticsAsync();
    }
}
