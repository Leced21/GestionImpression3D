using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class MaterialConsumptionRepository : IMaterialConsumptionRepository
    {
        private readonly AppDbContext _context;
        public MaterialConsumptionRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<MaterialConsumption> CreateAsync(MaterialConsumption consumption)
        {
            _context.MaterialConsumptions.Add(consumption);
            await _context.SaveChangesAsync();
            return consumption;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var consumption = await GetByIdAsync(id);
            if (consumption == null) return false;

            _context.MaterialConsumptions.Remove(consumption);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MaterialConsumption>> GetAllAsync()
        {
            return await _context.MaterialConsumptions
            .Include(c => c.MaterialStock)
            .Include(c => c.PrintJob)
            .Include(c => c.OrdreFabrication)
            .OrderByDescending(c => c.ConsumedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<MaterialConsumption>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.MaterialConsumptions
            .Include(c => c.MaterialStock)
            .Where(c => c.ConsumedAt >= start && c.ConsumedAt <= end)
            .OrderByDescending(c => c.ConsumedAt)
            .ToListAsync();
        }

        public async Task<MaterialConsumption?> GetByIdAsync(int id)
        {
            return await _context.MaterialConsumptions
    .Include(c => c.MaterialStock)
    .Include(c => c.PrintJob)
    .Include(c => c.OrdreFabrication)
    .Include(c => c.ConsumedByUser)
    .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<MaterialConsumption>> GetByMaterialAsync(int materialId)
        {
            return await _context.MaterialConsumptions
            .Include(c => c.MaterialStock)
            .Where(c => c.MaterialStockId == materialId)
            .OrderByDescending(c => c.ConsumedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<MaterialConsumption>> GetByPrintJobAsync(int printJobId)
        {
            return await _context.MaterialConsumptions
    .Include(c => c.MaterialStock)
    .Where(c => c.PrintJobId == printJobId)
    .OrderByDescending(c => c.ConsumedAt)
    .ToListAsync();
        }

        public async Task<Dictionary<MaterialConsumptionType, decimal>> GetConsumptionStatisticsAsync(DateTime? start = null, DateTime? end = null)
        {
            var query = _context.MaterialConsumptions.AsQueryable();

            if (start.HasValue)
                query = query.Where(c => c.ConsumedAt >= start.Value);
            if (end.HasValue)
                query = query.Where(c => c.ConsumedAt <= end.Value);

            var result = await query
                .GroupBy(c => c.Type)
                .Select(g => new { Type = g.Key, Total = g.Sum(c => c.Quantity) })
                .ToDictionaryAsync(g => g.Type, g => g.Total);

            return result;
        }

        public async Task<decimal> GetTotalConsumptionByMaterialAsync(int materialId, DateTime? start = null, DateTime? end = null)
        {
            var query = _context.MaterialConsumptions.Where(c => c.MaterialStockId == materialId);

            if (start.HasValue)
                query = query.Where(c => c.ConsumedAt >= start.Value);
            if (end.HasValue)
                query = query.Where(c => c.ConsumedAt <= end.Value);

            return await query.SumAsync(c => c.Quantity);
        }
    }
}
