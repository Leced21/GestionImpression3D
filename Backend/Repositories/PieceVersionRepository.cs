using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PieceVersionRepository : IPieceVersionRepository
    {
        private readonly AppDbContext _context;
        public PieceVersionRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<PieceVersion> CreateAsync(PieceVersion version)
        {
            _context.PieceVersions.Add(version);
            await _context.SaveChangesAsync();
            return version;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var version = await GetByIdAsync(id);
            if (version == null) return false;

            _context.PieceVersions.Remove(version);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PieceVersion?> GetByIdAsync(int id)
        {
            return await _context.PieceVersions
                .Include(v => v.Piece)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<PieceVersion>> GetByPieceIdAsync(int pieceId)
        {
            return await _context.PieceVersions
                .Where(v => v.PieceId == pieceId)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();
        }

        public async Task<PieceVersion?> GetLatestVersionAsync(int pieceId)
        {
            return await _context.PieceVersions
                .Where(v => v.PieceId == pieceId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetNextVersionNumberAsync(int pieceId)
        {
            var latest = await GetLatestVersionAsync(pieceId);
            return (latest?.VersionNumber ?? 0) + 1;
        }

        public async Task<PieceVersion> UpdateAsync(PieceVersion version)
        {
            _context.Entry(version).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return version;
        }
    }
}
