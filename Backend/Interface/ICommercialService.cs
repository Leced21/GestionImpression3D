using Backend.Models;

namespace Backend.Interface
{
    public interface ICommercialService
    {
        // Catalogue
        Task<IEnumerable<CatalogueItem>> GetCatalogueAsync();

        // Commandes
        Task<Commande> CreerCommandeAsync(CommandeRequest request);
        Task<Commande?> GetCommandeAsync(int id);
        Task<IEnumerable<Commande>> GetAllCommandesAsync();
        Task<IEnumerable<Commande>> GetByClientAsync(int clientId);
        Task<Commande?> UpdateStatutCommandeAsync(int id, string nouveauStatut);
        Task<bool> AnnulerCommandeAsync(int id);

        // Statistiques
        Task<decimal> GetChiffreAffairesAsync();
        Task<object> GetDashboardStatsAsync();
    }
}
