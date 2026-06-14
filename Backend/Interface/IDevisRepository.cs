using Backend.DTOs;
using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IDevisRepository
    {
        Task<Devis?> GetByIdAsync(int id);
        Task<IEnumerable<Devis>> GetAllAsync();
        Task<IEnumerable<Devis>> GetByClientAsync(int clientId);
        Task<IEnumerable<Devis>> GetByStatutAsync(DevisStatus statut);
        Task<Devis> CreateAsync(Devis devis);
        Task<Devis> UpdateAsync(Devis devis);
        Task<Devis?> UpdateStatutAsync(int id, DevisStatus statut);
        Task<bool> DeleteAsync(int id);
        Task<string> GenerateDevisNumberAsync();
        Task<DevisStatisticsDto> GetStatisticsAsync();
    }
}
