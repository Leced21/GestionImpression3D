using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IOrdreFabricationRepository
    {
        Task<OrdreFabrication?> GetByIdAsync(int id);
        Task<IEnumerable<OrdreFabrication>> GetAllAsync();
        Task<IEnumerable<OrdreFabrication>> GetByProjetAsync(int projetId);
        Task<IEnumerable<OrdreFabrication>> GetByStatutAsync(OrdreStatut statut);
        Task<OrdreFabrication> CreateAsync(OrdreFabrication ordre);
        Task<OrdreFabrication> UpdateAsync(OrdreFabrication ordre);
        Task<bool> DeleteAsync(int id);
        Task<int> GetNextReferenceNumberAsync();
    }
}
