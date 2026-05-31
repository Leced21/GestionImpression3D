using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetGlobalStatsAsync()
        {
            var piecesCount = await _context.Pieces.CountAsync();
            var projetsCount = await _context.Projets.CountAsync();
            var printersCount = await _context.Printers.CountAsync();
            var activePrinters = await _context.Printers.CountAsync(p => p.Status == PrinterStatus.Available);

            var totalJobs = await _context.PrintJobs.CountAsync();
            var completedJobs = await _context.PrintJobs.CountAsync(j => j.Status == PrintJobStatus.Completed);
            var failedJobs = await _context.PrintJobs.CountAsync(j => j.Status == PrintJobStatus.Failed);
            var pendingJobs = await _context.PrintJobs.CountAsync(j => j.Status == PrintJobStatus.Pending || j.Status == PrintJobStatus.Queued);
            var printingJobs = await _context.PrintJobs.CountAsync(j => j.Status == PrintJobStatus.Printing);

            var successRate = totalJobs > 0 ? (double)completedJobs / totalJobs * 100 : 0;

            var lowStockMaterials = await _context.MaterialStocks.CountAsync(m => m.Quantity <= m.MinThreshold && m.IsActive);
            var totalStockValue = await _context.MaterialStocks.Where(m => m.IsActive).SumAsync(m => m.Quantity * m.UnitPrice);

            return new
            {
                TotalPieces = piecesCount,
                TotalProjets = projetsCount,
                TotalPrinters = printersCount,
                ActivePrinters = activePrinters,
                PrintJobs = new
                {
                    TotalJobs = totalJobs,
                    CompletedJobs = completedJobs,
                    FailedJobs = failedJobs,
                    PendingJobs = pendingJobs,
                    PrintingJobs = printingJobs,
                    SuccessRate = Math.Round(successRate, 1)
                },
                MaterialStock = new
                {
                    TotalMaterials = await _context.MaterialStocks.CountAsync(m => m.IsActive),
                    LowStockMaterials = lowStockMaterials,
                    TotalValue = totalStockValue
                },
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<object> GetProductionTrendAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            var trends = await _context.PrintJobs
                .Where(j => j.CompletedAt.HasValue && j.CompletedAt >= startDate)
                .GroupBy(j => j.CompletedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Completed = g.Count(),
                    Failed = _context.PrintJobs.Count(j => j.CompletedAt.HasValue && j.CompletedAt.Value.Date == g.Key && j.Status == PrintJobStatus.Failed)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return trends;
        }

        public async Task<object> GetMaterialConsumptionAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            var consumption = await _context.MaterialStocks
                .Where(m => m.LastUsedAt.HasValue && m.LastUsedAt >= startDate && m.IsActive)
                .Select(m => new
                {
                    m.Name,
                    Type = m.Type.ToString(),
                    Quantity = m.Quantity,
                    Unit = m.Unit.ToString()
                })
                .ToListAsync();

            return consumption;
        }

        public async Task<object> GetPrintersActivityAsync()
        {
            var printers = await _context.Printers
                .Select(p => new
                {
                    p.Nom,
                    Status = p.Status.ToString(),
                    p.TotalPrintJobs,
                    p.TotalPrintHours,
                    LastPrint = p.LastPrint
                })
                .ToListAsync();

            return printers;
        }
    }
}
