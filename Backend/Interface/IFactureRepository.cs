using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IFactureRepository
    {
        Task<Facture?> GetByIdAsync(int id);
        Task<IEnumerable<Facture>> GetAllAsync();
        Task<IEnumerable<Facture>> GetByClientAsync(int clientId);
        Task<bool> ExistsForDevisAsync(int devisId);
        Task<Facture> CreateAsync(Facture facture);
        Task<Facture?> UpdateStatutAsync(int id, FactureStatus statut);
        Task<string> GenerateNumeroFactureAsync();
    }
}
