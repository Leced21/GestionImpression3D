using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class MaterialStockRepository : IMaterialStockRepository
    {
        private readonly AppDbContext _context;
        public MaterialStockRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<MaterialStock> CreateAsync(MaterialStock material)
        {
            _context.MaterialStocks.Add(material);
            await _context.SaveChangesAsync();
            return material;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var material = await GetByIdAsync(id);
            if (material == null) return false;

            _context.MaterialStocks.Remove(material);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MaterialStock>> GetAllAsync()
        {
            return await _context.MaterialStocks
                .OrderBy(m => m.Type)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<MaterialStock?> GetByIdAsync(int id)
        {
            return await _context.MaterialStocks.FindAsync(id);
        }

        public async Task<IEnumerable<MaterialStock>> GetByTypeAsync(MaterialType type)
        {
            return await _context.MaterialStocks
                .Where(m => m.Type == type && m.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaterialStock>> GetLowStockAsync()
        {
            return await _context.MaterialStocks
                .Where(m => m.Quantity <= m.MinThreshold && m.IsActive)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalValueAsync()
        {
            return await _context.MaterialStocks
                .Where(m => m.IsActive)
                .SumAsync(m => m.Quantity * m.UnitPrice);
        }

        public async Task<MaterialStock> UpdateAsync(MaterialStock material)
        {
            _context.Entry(material).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return material;
        }
    }
}
