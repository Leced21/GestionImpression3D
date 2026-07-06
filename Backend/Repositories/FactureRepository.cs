using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class FactureRepository : IFactureRepository
    {
        private readonly AppDbContext _context;
        public FactureRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Facture> CreateAsync(Facture facture)
        {
            facture.NumeroFacture = await GenerateNumeroFactureAsync();
            facture.CreatedAt = DateTime.UtcNow;
            _context.Factures.Add(facture);
            await _context.SaveChangesAsync();
            return facture;
        }

        public async Task<bool> ExistsForDevisAsync(int devisId)
        {
            return await _context.Factures.AnyAsync(f => f.DevisId == devisId);
        }

        public async Task<string> GenerateNumeroFactureAsync()
        {
            var year = DateTime.Now.Year;
            var lastFacture = await _context.Factures
                .Where(f => f.NumeroFacture.StartsWith($"FACT-{year}"))
                .OrderByDescending(f => f.NumeroFacture)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastFacture != null)
            {
                var lastNumber = int.Parse(lastFacture.NumeroFacture.Split('-').Last());
                nextNumber = lastNumber + 1;
            }

            return $"FACT-{year}-{nextNumber:D4}";
        }

        public async Task<IEnumerable<Facture>> GetAllAsync()
        {
            return await _context.Factures
                        .Include(f => f.Client)
                        .Include(f => f.Lignes)
                        .OrderByDescending(f => f.DateEmission)
                        .ToListAsync();
        }

        public async Task<IEnumerable<Facture>> GetByClientAsync(int clientId)
        {
            return await _context.Factures
                        .Include(f => f.Lignes)
                        .Where(f => f.ClientId == clientId)
                        .OrderByDescending(f => f.DateEmission)
                        .ToListAsync();
        }

        public async Task<Facture?> GetByIdAsync(int id)
        {
            return await _context.Factures
                        .Include(f => f.Client)
                        .Include(f => f.Devis)
                        .Include(f => f.Lignes)
                        .ThenInclude(l => l.Piece)
                        .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Facture?> UpdateStatutAsync(int id, FactureStatus statut)
        {
            var facture = await GetByIdAsync(id);
            if (facture == null) return null;

            facture.Statut = statut;
            await _context.SaveChangesAsync();
            return facture;
        }
    }
}
