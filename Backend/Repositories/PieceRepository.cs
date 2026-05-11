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
