using Backend.Data;
using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PrinterMaintenanceRepository : IPrinterMaintenanceRepository
    {
        private readonly AppDbContext _context;
        public PrinterMaintenanceRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<PrinterMaintenance?> CompleteAsync(int id, string? notes = null, string? performedBy = null)
        {
            var maintenance = await GetByIdAsync(id);
            if (maintenance == null) return null;

            maintenance.Status = MaintenanceStatus.Completed;
            maintenance.CompletedDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(notes))
                maintenance.Notes = notes;
            if (!string.IsNullOrEmpty(performedBy))
                maintenance.PerformedBy = performedBy;

            await _context.SaveChangesAsync();
            return maintenance;
        }

        public async Task<PrinterMaintenance> CreateAsync(PrinterMaintenance maintenance)
        {
            _context.PrinterMaintenances.Add(maintenance);
            await _context.SaveChangesAsync();
            return maintenance;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var maintenance = await GetByIdAsync(id);
            if (maintenance == null) return false;

            _context.PrinterMaintenances.Remove(maintenance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PrinterMaintenance>> GetAllAsync()
        {
            return await _context.PrinterMaintenances
                            .Include(m => m.Printer)
                            .OrderByDescending(m => m.ScheduledDate)
                            .ToListAsync();
        }

        public async Task<PrinterMaintenance?> GetByIdAsync(int id)
        {
            return await _context.PrinterMaintenances
                            .Include(m => m.Printer)
                            .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<PrinterMaintenance>> GetByPrinterAsync(int printerId)
        {
            return await _context.PrinterMaintenances
                            .Include(m => m.Printer)
                            .Where(m => m.PrinterId == printerId)
                            .OrderByDescending(m => m.ScheduledDate)
                            .ToListAsync();
        }

        public async Task<PrinterMaintenanceStatisticsDto> GetStatisticsAsync(int printerId)
        {
            var maintenances = await _context.PrinterMaintenances
                                    .Where(m => m.PrinterId == printerId)
                                    .ToListAsync();

            var completed = maintenances.Where(m => m.Status == MaintenanceStatus.Completed).ToList();
            var pending = maintenances.Where(m => m.Status == MaintenanceStatus.Scheduled || m.Status == MaintenanceStatus.InProgress).ToList();

            return new PrinterMaintenanceStatisticsDto
            {
                TotalMaintenances = maintenances.Count,
                PreventiveCount = maintenances.Count(m => m.Type == MaintenanceType.Preventive),
                CorrectiveCount = maintenances.Count(m => m.Type == MaintenanceType.Corrective),
                CompletedCount = completed.Count,
                PendingCount = pending.Count,
                TotalCost = completed.Sum(m => m.Cost),
                AverageDurationMinutes = completed.Any() ? (decimal)completed.Average(m => m.DurationMinutes) : 0,
                LastMaintenance = completed.MaxBy(m => m.CompletedDate)?.CompletedDate,
                NextScheduled = pending.MinBy(m => m.ScheduledDate)?.ScheduledDate
            };
        }

        public async Task<IEnumerable<PrinterMaintenance>> GetUpcomingAsync(int days = 7)
        {
            var cutoff = DateTime.UtcNow.AddDays(days);
            return await _context.PrinterMaintenances
                .Include(m => m.Printer)
                .Where(m => m.ScheduledDate <= cutoff && m.Status == MaintenanceStatus.Scheduled)
                .OrderBy(m => m.ScheduledDate)
                .ToListAsync();
        }

        public async Task<PrinterMaintenance> UpdateAsync(PrinterMaintenance maintenance)
        {
            _context.Entry(maintenance).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return maintenance;
        }
    }
}
