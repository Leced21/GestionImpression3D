using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Interface;
using Backend.Models;

namespace Backend.Repositories
{
    public class PieceRepository : IPieceRepository
    {
        private readonly AppDbContext _context;
        public PieceRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<decimal> CalculerPrixRecommandéAsync(int id)
        {
            var piece = await _context.Pieces.FindAsync(id);
            if ( piece == null)
            {
                return 0;
            }
            return piece.CoutTotal * 1.3m; // Exemple de calcul : coût total + 30% de marge
        }

        public async Task<Piece> CreateAsync(Piece piece)
        {
            piece.DateCreation = DateTime.UtcNow;
            piece.Statut = "Brouillon"; // Statut par défaut
            _context.Pieces.Add(piece);
            await _context.SaveChangesAsync();
            return piece;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var piece = await  _context.Pieces.FindAsync(id);
            if (piece == null)
            {
                return false;
            }
            _context.Pieces.Remove(piece);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Piece>> GetAllAsync()
        {
            return await _context.Pieces.ToListAsync();
        }

        public async Task<Piece?> GetByIdAsync(int id)
        {
            return await _context.Pieces.FindAsync(id);
        }

        public async Task<Piece?> UpdateAsync(int id, Piece piece)
        {
            var existingPiece = await _context.Pieces.FindAsync(id);
            if (existingPiece == null)
            {
                return null;
            }
            // Mettre à jour les propriétés de la pièce existante avec les valeurs de la nouvelle pièce
            existingPiece.Nom = piece.Nom;
            existingPiece.Reference = piece.Reference;
            existingPiece.Description = piece.Description;
            existingPiece.Statut = piece.Statut;
            existingPiece.CoutMatiere = piece.CoutMatiere;
            existingPiece.CoutMachine = piece.CoutMachine;
            existingPiece.CoutMainOeuvre = piece.CoutMainOeuvre;
            existingPiece.PrixVente = piece.PrixVente;
            existingPiece.StlFileName = piece.StlFileName;
            existingPiece.DateModification = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingPiece;
        }

        public async Task<Piece?> UpdateStatutAsync(int id, string nouveauStatut)
        {
            var piece = await _context.Pieces.FindAsync(id);
            if (piece == null)
            {
                return null;
            }
            var statutsValides = new [] { "Brouillon", "Conception", "Prototypage", "Validation", "Production", "Commercialisable" };
            if (!statutsValides.Contains(nouveauStatut))
            {
                throw new ArgumentException("Statut invalide");
            }
            piece.Statut = nouveauStatut;
            piece.DateModification = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return piece;
        }
    }
}
