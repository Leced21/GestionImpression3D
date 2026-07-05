using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class CommercialRepository : ICommercialRepository
    {
        private readonly AppDbContext _context;
        public CommercialRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<CommandeLigne> AddLigneAsync(CommandeLigne ligne)
        {
            _context.CommandeLignes.Add(ligne);
            await _context.SaveChangesAsync();
            return ligne;
        }

        public async Task<Commande> CreateAsync(Commande commande)
        {
            _context.Commandes.Add(commande);
            await _context.SaveChangesAsync();
            return commande;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var commande = await _context.Commandes.FindAsync(id);
            if (commande == null) return false;

            _context.Commandes.Remove(commande);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Commande>> GetAllAsync()
        {
            return await _context.Commandes
                                .Include(c => c.Lignes)
                                .OrderByDescending(c => c.DateCommande)
                                .ToListAsync();
        }

        public async Task<IEnumerable<Commande>> GetByClientAsync(int clientId)
        {
            return await _context.Commandes
                .Include(c => c.Lignes)
                .Where(c => c.ClientId == clientId)
                .OrderByDescending(c => c.DateCommande)
                .ToListAsync();
        }

        public async Task<Commande?> GetByIdAsync(int id)
        {
            return await _context.Commandes
                                .Include(c => c.Lignes)
                                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Commande>> GetByStatutAsync(string statut)
        {
            return await _context.Commandes
                                 .Include(c => c.Lignes)
                                 .Where(c => c.Statut == statut)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<CatalogueItem>> GetCatalogueAsync()
        {
            var pieces = await _context.Pieces
                .Where(p => p.Statut == PieceStatus.Commercialisable && p.PrixVente > 0 && p.EstDisponible && p.Stock > 0)
                .Select(p => new CatalogueItem
                {
                    Id = p.Id,
                    Nom = p.Nom,
                    Reference = p.Reference,
                    Description = p.Description,
                    PrixVente = p.PrixVente,
                    Categorie = p.Categorie ?? "Mécanique",
                    Materiau = p.Materiau ?? "PLA",
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return pieces;
        }

        public async Task<decimal> GetChiffreAffairesAsync()
        {
            return await _context.Commandes
                .Where(c => c.Statut == "Livrée")
                .SumAsync(c => c.Total);
        }

        public async Task<Dictionary<string, int>> GetStatistiquesCommandesAsync()
        {
            var stats = await _context.Commandes
                .GroupBy(c => c.Statut)
                .Select(g => new { Statut = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Statut, g => g.Count);

            return stats;
        }

        public async Task<Commande?> UpdateStatutAsync(int id, string nouveauStatut)
        {
            var commande = await _context.Commandes.FindAsync(id);
            if (commande == null) return null;

            commande.Statut = nouveauStatut;
            if (nouveauStatut == "Livrée")
                commande.DateLivraison = DateTime.Now;

            await _context.SaveChangesAsync();
            return commande;
        }

        public async Task<bool> UpdateStockAsync(int pieceId, int quantite)
        {
            var piece = await _context.Pieces.FindAsync(pieceId);
            if (piece == null) return false;

            piece.Stock -= quantite;
            if (piece.Stock < 0) piece.Stock = 0;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreStockAsync(int pieceId, int quantite)
        {
            var piece = await _context.Pieces.FindAsync(pieceId);
            if (piece == null) return false;

            piece.Stock += quantite;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateNumeroCommandeAsync()
        {
            var year = DateTime.Now.Year;
            var lastCommande = await _context.Commandes
                .Where(c => c.NumeroCommande.StartsWith($"CMD-{year}"))
                .OrderByDescending(c => c.NumeroCommande)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastCommande != null)
            {
                var lastNumber = int.Parse(lastCommande.NumeroCommande.Split('-').Last());
                nextNumber = lastNumber + 1;
            }

            return $"CMD-{year}-{nextNumber:D4}";
        }
    }
}
