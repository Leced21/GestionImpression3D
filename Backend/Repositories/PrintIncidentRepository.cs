using Backend.Data;
using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PrintIncidentRepository : IPrintIncidentRepository
    {
        private readonly AppDbContext _context;
        public PrintIncidentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PrintIncident> CreateAsync(PrintIncident incident)
        {
            _context.PrintIncidents.Add(incident);
            await _context.SaveChangesAsync();
            return incident;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var incident = await GetByIdAsync(id);
            if (incident == null) return false;

            _context.PrintIncidents.Remove(incident);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PrintIncident>> GetAllAsync()
        {
            return await _context.PrintIncidents
                        .Include(i => i.Printer)
                        .OrderByDescending(i => i.OccurredAt)
                        .ToListAsync();
        }

        public async Task<PrintIncident?> GetByIdAsync(int id)
        {
            return await _context.PrintIncidents
            .Include(i => i.PrintJob)
            .Include(i => i.Printer)
            .Include(i => i.ReportedByUser)
            .Include(i => i.ResolvedByUser)
            .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<PrintIncident>> GetByPrinterAsync(int printerId)
        {
            return await _context.PrintIncidents
                        .Include(i => i.Printer)
                        .Where(i => i.PrinterId == printerId)
                        .OrderByDescending(i => i.OccurredAt)
                        .ToListAsync();
        }

        public async Task<IEnumerable<PrintIncident>> GetByPrintJobAsync(int printJobId)
        {
            return await _context.PrintIncidents
                            .Where(i => i.PrintJobId == printJobId)
                            .OrderByDescending(i => i.OccurredAt)
                            .ToListAsync();
        }

        public async Task<IEnumerable<PrintIncident>> GetByStatusAsync(IncidentStatus status)
        {
            return await _context.PrintIncidents
                        .Include(i => i.Printer)
                        .Where(i => i.Status == status)
                        .OrderByDescending(i => i.OccurredAt)
                        .ToListAsync();
        }

        public async Task<IncidentStatisticsDto> GetStatisticsAsync(DateTime? start = null, DateTime? end = null)
        {
            var query = _context.PrintIncidents.AsQueryable();

            if (start.HasValue)
                query = query.Where(i => i.OccurredAt >= start.Value);
            if (end.HasValue)
                query = query.Where(i => i.OccurredAt <= end.Value);

            var incidents = await query.ToListAsync();

            var resolved = incidents.Where(i => i.Status == IncidentStatus.Résolu && i.ResolvedAt.HasValue).ToList();
            var avgResolutionHours = resolved.Any()
                ? resolved.Average(i => (i.ResolvedAt!.Value - i.OccurredAt).TotalHours)
                : 0;

            return new IncidentStatisticsDto
            {
                TotalIncidents = incidents.Count,
                OpenIncidents = incidents.Count(i => i.Status == IncidentStatus.Ouvert),
                InProgressIncidents = incidents.Count(i => i.Status == IncidentStatus.EnCours),
                ResolvedIncidents = incidents.Count(i => i.Status == IncidentStatus.Résolu),
                ClosedIncidents = incidents.Count(i => i.Status == IncidentStatus.Fermé),
                BySeverity = incidents.GroupBy(i => i.Severity).ToDictionary(g => g.Key, g => g.Count()),
                ByPrinter = incidents.Where(i => i.PrinterId.HasValue).GroupBy(i => i.PrinterId!.Value.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageResolutionTimeHours = Math.Round(avgResolutionHours, 1)
            };
        }

        public async Task<PrintIncident?> ResolveAsync(int id, string resolution, int resolvedBy)
        {
            var incident = await GetByIdAsync(id);
            if (incident == null) return null;

            incident.Status = IncidentStatus.Résolu;
            incident.ResolvedAt = DateTime.UtcNow;
            incident.Resolution = resolution;
            incident.ResolvedBy = resolvedBy;

            await _context.SaveChangesAsync();
            return incident;
        }

        public async Task<PrintIncident> UpdateAsync(PrintIncident incident)
        {
            _context.Entry(incident).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return incident;
        }
    }
}
