using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PrintProfileRepository : IPrintProfileRepository
    {
        private readonly AppDbContext _context;
        public PrintProfileRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<PrintProfile> CreateAsync(PrintProfile profile)
        {
            _context.PrintProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var profile = await GetByIdAsync(id);
            if (profile == null) return false;

            _context.PrintProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PrintProfile>> GetAllAsync()
        {
            return await _context.PrintProfiles
                .Include(p => p.Printer)
                .OrderBy(p => p.Nom)
                .ToListAsync();
        }

        public async Task<PrintProfile?> GetByIdAsync(int id)
        {
            return await _context.PrintProfiles
                .Include(p => p.Printer)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PrintProfile>> GetByMateriauAsync(string materiau)
        {
            return await _context.PrintProfiles
                .Include(p => p.Printer)
                .Where(p => p.Materiau == materiau && p.IsActive)
                .OrderBy(p => p.Nom)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrintProfile>> GetByPrinterAsync(int printerId)
        {
            return await _context.PrintProfiles
                .Include(p => p.Printer)
                .Where(p => p.PrinterId == printerId && p.IsActive)
                .OrderBy(p => p.Nom)
                .ToListAsync();
        }

        public async Task<PrintProfile?> GetDefaultForPrinterAsync(int printerId)
        {
            return await _context.PrintProfiles
                .Include(p => p.Printer)
                .FirstOrDefaultAsync(p => p.PrinterId == printerId && p.IsDefault && p.IsActive);
        }

        public async Task<bool> SetDefaultAsync(int printerId, int profileId)
        {
            // Désactiver tous les autres profils par défaut pour cette imprimante
            var defaultProfiles = await _context.PrintProfiles
                .Where(p => p.PrinterId == printerId && p.IsDefault)
                .ToListAsync();

            foreach (var profile in defaultProfiles)
            {
                profile.IsDefault = false;
            }

            // Activer le nouveau profil par défaut
            var newDefault = await _context.PrintProfiles.FindAsync(profileId);
            if (newDefault == null) return false;

            newDefault.IsDefault = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PrintProfile> UpdateAsync(PrintProfile profile)
        {
            _context.Entry(profile).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return profile;
        }
    }
}
