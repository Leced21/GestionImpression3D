using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ProjetRepository : IProjetRepository
    {
        private readonly AppDbContext _context;
        public ProjetRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ProjetPiece> AddPieceAsync(int projetId, int pieceId, int quantite)
        {
            // Vérifier si la pièce est déjà liée au projet
            var existing = await _context.ProjetPieces
                .FirstOrDefaultAsync(pp => pp.ProjetId == projetId && pp.PieceId == pieceId);

            if (existing != null)
            {
                existing.Quantite += quantite;
                // Mettre à jour la date d'ajout pour refléter la modification
                existing.DateAjout = DateTime.Now;
                await _context.SaveChangesAsync();
                return existing;
            }

            var projetPiece = new ProjetPiece
            {
                ProjetId = projetId,
                PieceId = pieceId,
                Quantite = quantite,
                DateAjout = DateTime.Now
            };

            _context.ProjetPieces.Add(projetPiece);
            await _context.SaveChangesAsync();
            return projetPiece;
        }

        public async Task<Projet> CreateAsync(Projet projet)
        {
            if (string.IsNullOrEmpty(projet.Reference))
            {
                projet.Reference = $"PRJ-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000, 10000)}";
            }

            _context.Projets.Add(projet);
            await _context.SaveChangesAsync();
            return projet;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var projet = await _context.Projets.FindAsync(id);
            if (projet == null) return false;

            _context.Projets.Remove(projet);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Projet>> GetAllAsync()
        {
            return await _context.Projets
                .Include(p => p.ProjetPieces)
                .ThenInclude(pp => pp.Piece)
                .OrderByDescending(p => p.DateCreation)
                .ToListAsync();
        }

        public async Task<Projet?> GetByIdAsync(int id)
        {
            return await _context.Projets
                .Include(p => p.ProjetPieces)
                .ThenInclude(pp => pp.Piece)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ProjetPiece>> GetPiecesByProjetAsync(int projetId)
        {
            return await _context.ProjetPieces
                .Include(pp => pp.Piece)
                .Where(pp => pp.ProjetId == projetId)
                .ToListAsync();
        }

        public async Task<bool> RemovePieceAsync(int projetId, int pieceId)
        {
            var projetPiece = await _context.ProjetPieces
                    .FirstOrDefaultAsync(pp => pp.ProjetId == projetId && pp.PieceId == pieceId);

            if (projetPiece == null) return false;

            _context.ProjetPieces.Remove(projetPiece);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Projet?> UpdateAsync(int id, Projet projet)
        {
            var existing = await _context.Projets.FindAsync(id);
            if (existing == null) return null;

            existing.Nom = projet.Nom;
            existing.Description = projet.Description;
            existing.Statut = projet.Statut;
            existing.DateLivraisonPrevue = projet.DateLivraisonPrevue;
            existing.ClientNom = projet.ClientNom;
            existing.ClientEmail = projet.ClientEmail;
            existing.Budget = projet.Budget;

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
