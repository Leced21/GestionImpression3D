using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface ICommandeReader
    {
        Task<Commande?> GetByIdAsync(int id);
        Task<IEnumerable<Commande>> GetAllAsync();
        Task<IEnumerable<Commande>> GetByStatutAsync(CommandeStatus statut);
        Task<IEnumerable<Commande>> GetByClientAsync(int clientId);
    }

    public interface ICommandeWriter
    {
        Task<Commande> CreateAsync(Commande commande);
        Task<CommandeLigne> AddLigneAsync(CommandeLigne ligne);
        Task<Commande?> UpdateStatutAsync(int id, CommandeStatus nouveauStatut);
        Task<bool> DeleteAsync(int id);
    }
    public interface ICommercialRepository : ICommandeReader, ICommandeWriter
    {
        Task<IEnumerable<CatalogueItem>> GetCatalogueAsync();
        Task<bool> UpdateStockAsync(int pieceId, int quantite);
        Task<bool> RestoreStockAsync(int pieceId, int quantite);
        Task<decimal> GetChiffreAffairesAsync();
        Task<Dictionary<CommandeStatus, int>> GetStatistiquesCommandesAsync();
        Task<string> GenerateNumeroCommandeAsync();
    }
}
