using Backend.Interface;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class CommercialService : ICommercialService
    {
        private readonly IPieceRepository _pieceRepository;
        private readonly ICommercialRepository _repository;
        public CommercialService(IPieceRepository pieceRepository, ICommercialRepository repository)
        {
            _pieceRepository = pieceRepository;
            _repository = repository;
        }
        public async Task<bool> AnnulerCommandeAsync(int id)
        {
            var commande = await _repository.GetByIdAsync(id);
            if (commande == null) return false;

            if (commande.Statut != "En attente" && commande.Statut != "Confirmée")
                throw new InvalidOperationException("Seules les commandes en attente ou confirmées peuvent être annulées");

            return await _repository.DeleteAsync(id);
        }

        public async Task<Commande> CreerCommandeAsync(CommandeRequest request)
        {
            // Validation
            if (request.Items == null || !request.Items.Any())
                throw new InvalidOperationException("La commande doit contenir au moins un article");

            // Vérifier les stocks
            foreach (var item in request.Items)
            {
                var piece = await _pieceRepository.GetByIdAsync(item.PieceId);
                if (piece == null)
                    throw new InvalidOperationException($"Pièce {item.PieceId} introuvable");

                if (piece.Stock < item.Quantite)
                    throw new InvalidOperationException($"Stock insuffisant pour {piece.Nom}. Disponible: {piece.Stock}");
            }

            // Générer numéro de commande
            var numeroCommande = GenererNumeroCommande();

            // Calculer le total
            decimal total = 0;
            var lignes = new List<CommandeLigne>();

            foreach (var item in request.Items)
            {
                var piece = await _pieceRepository.GetByIdAsync(item.PieceId);
                var ligne = new CommandeLigne
                {
                    PieceId = item.PieceId,
                    Nom = piece!.Nom,
                    Reference = piece.Reference,
                    Quantite = item.Quantite,
                    PrixUnitaire = piece.PrixVente
                };
                lignes.Add(ligne);
                total += ligne.Total;

                // Mettre à jour le stock
                await _repository.UpdateStockAsync(item.PieceId, item.Quantite);
            }

            var commande = new Commande
            {
                NumeroCommande = numeroCommande,
                ClientNom = request.ClientNom,
                ClientEmail = request.ClientEmail,
                ClientTelephone = request.ClientTelephone,
                AdresseLivraison = request.AdresseLivraison,
                Total = total,
                Statut = "En attente",
                DateCommande = DateTime.Now,
                Notes = request.Notes,
                Lignes = lignes
            };

            return await _repository.CreateAsync(commande);
        }

        public async Task<IEnumerable<Commande>> GetAllCommandesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<CatalogueItem>> GetCatalogueAsync()
        {
            return await _repository.GetCatalogueAsync();
        }

        public async Task<decimal> GetChiffreAffairesAsync()
        {
            return await _repository.GetChiffreAffairesAsync();
        }

        public async Task<Commande?> GetCommandeAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<object> GetDashboardStatsAsync()
        {
            var commandes = await _repository.GetAllAsync();
            var statsParStatut = await _repository.GetStatistiquesCommandesAsync();
            var ca = await _repository.GetChiffreAffairesAsync();

            return new
            {
                TotalCommandes = commandes.Count(),
                CommandesEnAttente = statsParStatut.GetValueOrDefault("En attente", 0),
                CommandesEnProduction = statsParStatut.GetValueOrDefault("En production", 0),
                CommandesLivrees = statsParStatut.GetValueOrDefault("Livrée", 0),
                ChiffreAffaires = ca,
                ChiffreAffairesMois = await GetChiffreAffairesMoisAsync()
            };
        }

        public async Task<Commande?> UpdateStatutCommandeAsync(int id, string nouveauStatut)
        {
            var statutsValides = new[] { "En attente", "Confirmée", "En production", "Expédiée", "Livrée" };
            if (!statutsValides.Contains(nouveauStatut))
                throw new ArgumentException("Statut invalide");

            return await _repository.UpdateStatutAsync(id, nouveauStatut);
        }

        // Méthodes privées
        private string GenererNumeroCommande()
        {
            return $"CMD-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000, 10000)}";
        }

        private async Task<decimal> GetChiffreAffairesMoisAsync()
        {
            var commandes = await _repository.GetAllAsync();
            return commandes
                .Where(c => c.Statut == "Livrée" &&
                            c.DateCommande.Month == DateTime.Now.Month &&
                            c.DateCommande.Year == DateTime.Now.Year)
                .Sum(c => c.Total);
        }
    }
}
