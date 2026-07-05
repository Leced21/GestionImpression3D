using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class CommercialService : ICommercialService
    {
        private readonly IPieceRepository _pieceRepository;
        private readonly ICommercialRepository _repository;
        private readonly IClientService _clientService;
        private readonly IAuditLogger _auditLogger;
        public CommercialService(IPieceRepository pieceRepository, ICommercialRepository repository, IClientService clientService, IAuditLogger auditLogger)
        {
            _pieceRepository = pieceRepository;
            _repository = repository;
            _clientService = clientService;
            _auditLogger = auditLogger;
        }
        public async Task<bool> AnnulerCommandeAsync(int id)
        {
            var commande = await _repository.GetByIdAsync(id);
            if (commande == null) return false;

            if (commande.Statut != "En attente" && commande.Statut != "Confirmée")
                throw new InvalidOperationException("Seules les commandes en attente ou confirmées peuvent être annulées");

            await UpdateStatutCommandeAsync(id, "Annulée");
            return true;
        }

        public async Task<Commande> CreerCommandeAsync(CommandeRequest request)
        {
            // Validation
            if (request.Items == null || !request.Items.Any())
                throw new InvalidOperationException("La commande doit contenir au moins un article");

            var client = await _clientService.EnsureClientAsync(new Backend.DTOs.CreateClientRequest
            {
                Nom = request.ClientNom,
                Email = request.ClientEmail,
                Telephone = request.ClientTelephone,
                Adresse = request.AdresseLivraison
            });

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
            var numeroCommande = await _repository.GenerateNumeroCommandeAsync();

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
                ClientId = client?.Id,
                ClientNom = client?.Nom ?? request.ClientNom,
                ClientEmail = client?.Email ?? request.ClientEmail,
                ClientTelephone = client?.Telephone ?? request.ClientTelephone,
                AdresseLivraison = request.AdresseLivraison,
                Total = total,
                Statut = "En attente",
                DateCommande = DateTime.Now,
                Notes = request.Notes,
                Lignes = lignes
            };

            var created = await _repository.CreateAsync(commande);

            await _auditLogger.LogCreationAsync(EntityType.Commande, created.Id, created.NumeroCommande);

            return created;
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

        public async Task<IEnumerable<Commande>> GetByClientAsync(int clientId)
        {
            return await _repository.GetByClientAsync(clientId);
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
            var statutsValides = new[] { "En attente", "Confirmée", "En production", "Expédiée", "Livrée", "Annulée" };
            if (!statutsValides.Contains(nouveauStatut))
                throw new ArgumentException("Statut invalide");

            var commande = await _repository.GetByIdAsync(id);
            if (commande == null) return null;

            var ancienStatut = commande.Statut;
            if (ancienStatut == nouveauStatut) return commande;

            // Restituer le stock consommé quand une commande passe (pour la première fois) à Annulée,
            // quel que soit le chemin emprunté (annulation dédiée ou changement de statut direct).
            if (nouveauStatut == "Annulée")
            {
                foreach (var ligne in commande.Lignes)
                {
                    await _repository.RestoreStockAsync(ligne.PieceId, ligne.Quantite);
                }
            }

            var updated = await _repository.UpdateStatutAsync(id, nouveauStatut);

            await _auditLogger.LogStatusChangeAsync(EntityType.Commande, id, ancienStatut, nouveauStatut);

            return updated;
        }

        // Méthodes privées
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
