using Backend.DTOs;
using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IOrdreFabricationService
    {
        Task<IEnumerable<OrdreFabrication>> GetAllAsync();
        Task<OrdreFabrication?> GetByIdAsync(int id);
        Task<OrdreFabrication> CreateAsync(CreateOrdreRequest request);
        Task<OrdreFabrication?> UpdateAsync(int id, UpdateOrdreRequest request);
        Task<OrdreFabrication?> UpdateStatutAsync(int id, OrdreStatut statut);
        Task<OrdreFabrication?> StartProductionAsync(int id);
        Task<OrdreFabrication?> CompleteProductionAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<OrdreStatisticsDto> GetStatisticsAsync();
        Task<bool> ExistsForDevisAsync(int devisId);
    }
}
