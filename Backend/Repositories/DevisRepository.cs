using Backend.Data;
using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class DevisRepository : IDevisRepository
    {
        private readonly AppDbContext _context;
        public DevisRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Devis> CreateAsync(Devis devis)
        {
            devis.NumeroDevis = await GenerateDevisNumberAsync();
            devis.CreatedAt = DateTime.UtcNow;
            _context.Devis.Add(devis);
            await _context.SaveChangesAsync();
            return devis;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var devis = await GetByIdAsync(id);
            if (devis == null) return false;

            _context.Devis.Remove(devis);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateDevisNumberAsync()
        {
            var year = DateTime.Now.Year;
            var lastDevis = await _context.Devis
                .Where(d => d.NumeroDevis.StartsWith($"DEV-{year}"))
                .OrderByDescending(d => d.NumeroDevis)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastDevis != null)
            {
                var lastNumber = int.Parse(lastDevis.NumeroDevis.Split('-').Last());
                nextNumber = lastNumber + 1;
            }

            return $"DEV-{year}-{nextNumber:D4}";
        }

        public async Task<IEnumerable<Devis>> GetAllAsync()
        {
            return await _context.Devis
                        .Include(d => d.Client)
                        .Include(d => d.Lignes)
                        .OrderByDescending(d => d.DateEmission)
                        .ToListAsync();
        }

        public async Task<IEnumerable<Devis>> GetByClientAsync(int clientId)
        {
            return await _context.Devis
                        .Include(d => d.Client)
                        .Include(d => d.Lignes)
                        .Where(d => d.ClientId == clientId)
                        .OrderByDescending(d => d.DateEmission)
                        .ToListAsync();
        }

        public async Task<Devis?> GetByIdAsync(int id)
        {
            return await _context.Devis
                        .Include(d => d.Client)
                        .Include(d => d.Projet)
                        .Include(d => d.Lignes)
                        .ThenInclude(l => l.Piece)
                        .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Devis>> GetByStatutAsync(DevisStatus statut)
        {
            return await _context.Devis
                        .Include(d => d.Client)
                        .Where(d => d.Statut == statut)
                        .OrderByDescending(d => d.DateEmission)
                        .ToListAsync();
        }

        public async Task<DevisStatisticsDto> GetStatisticsAsync()
        {
            var devis = await _context.Devis.ToListAsync();

            return new DevisStatisticsDto
            {
                TotalDevis = devis.Count,
                BrouillonCount = devis.Count(d => d.Statut == DevisStatus.Brouillon),
                EnvoyesCount = devis.Count(d => d.Statut == DevisStatus.Envoyé),
                AcceptesCount = devis.Count(d => d.Statut == DevisStatus.Accepté),
                RefusesCount = devis.Count(d => d.Statut == DevisStatus.Refusé),
                ExpiresCount = devis.Count(d => d.Statut == DevisStatus.Expiré),
                TotalAmountAccepted = devis.Where(d => d.Statut == DevisStatus.Accepté).Sum(d => d.TotalTTC),
                AverageAmount = devis.Any() ? devis.Average(d => d.TotalTTC) : 0
            };
        }

        public async Task<Devis> UpdateAsync(Devis devis)
        {
            _context.Entry(devis).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return devis;
        }

        public async Task<Devis?> UpdateStatutAsync(int id, DevisStatus statut)
        {
            var devis = await GetByIdAsync(id);
            if (devis == null) return null;

            devis.Statut = statut;
            await _context.SaveChangesAsync();
            return devis;
        }
    }
}
