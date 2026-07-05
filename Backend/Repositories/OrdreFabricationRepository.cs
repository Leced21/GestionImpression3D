using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class OrdreFabricationRepository : IOrdreFabricationRepository
    {
        private readonly AppDbContext _context;
        public OrdreFabricationRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<OrdreFabrication> CreateAsync(OrdreFabrication ordre)
        {
            _context.OrdresFabrication.Add(ordre);
            await _context.SaveChangesAsync();
            return ordre;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ordre = await GetByIdAsync(id);
            if (ordre == null) return false;

            _context.OrdresFabrication.Remove(ordre);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<OrdreFabrication>> GetAllAsync()
        {
            return await _context.OrdresFabrication
                .Include(o => o.Projet)
                .Include(o => o.Piece)
                .OrderByDescending(o => o.DateCreation)
                .ToListAsync();
        }

        public async Task<OrdreFabrication?> GetByIdAsync(int id)
        {
            return await _context.OrdresFabrication
                .Include(o => o.Projet)
                .Include(o => o.Piece)
                .Include(o => o.PrintJobs)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<OrdreFabrication>> GetByProjetAsync(int projetId)
        {
            return await _context.OrdresFabrication
                .Include(o => o.Piece)
                .Where(o => o.ProjetId == projetId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrdreFabrication>> GetByStatutAsync(OrdreStatut statut)
        {
            return await _context.OrdresFabrication
                .Include(o => o.Projet)
                .Include(o => o.Piece)
                .Where(o => o.Statut == statut)
                .OrderBy(o => o.Priorite)
                .ThenBy(o => o.DateEcheance)
                .ToListAsync();
        }

        public async Task<int> GetNextReferenceNumberAsync()
        {
            var references = await _context.OrdresFabrication
                .Select(o => o.Reference)
                .ToListAsync();

            var maxNumber = 0;
            foreach (var reference in references)
            {
                if (reference.StartsWith("OF-", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(reference[3..], out var number) &&
                    number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            return maxNumber + 1;
        }

        public async Task<OrdreFabrication> UpdateAsync(OrdreFabrication ordre)
        {
            _context.Entry(ordre).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return ordre;
        }

        public async Task<bool> ExistsForDevisAsync(int devisId)
        {
            return await _context.OrdresFabrication.AnyAsync(o => o.DevisId == devisId);
        }
    }
}
