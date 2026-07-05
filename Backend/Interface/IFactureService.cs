using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IFactureService
    {
        Task<Facture?> GetByIdAsync(int id);
        Task<IEnumerable<Facture>> GetAllAsync();
        Task<IEnumerable<Facture>> GetByClientAsync(int clientId);
        Task<Facture?> UpdateStatutAsync(int id, FactureStatus statut);
        Task<byte[]> GeneratePdfAsync(int id);
        Task<bool> ExistsForDevisAsync(int devisId);
        Task<Facture> CreateFromDevisAsync(Devis devis);
    }
}
