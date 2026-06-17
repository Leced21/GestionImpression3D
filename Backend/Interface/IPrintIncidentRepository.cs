using Backend.DTOs;
using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IPrintIncidentRepository
    {
        Task<PrintIncident?> GetByIdAsync(int id);
        Task<IEnumerable<PrintIncident>> GetAllAsync();
        Task<IEnumerable<PrintIncident>> GetByPrinterAsync(int printerId);
        Task<IEnumerable<PrintIncident>> GetByPrintJobAsync(int printJobId);
        Task<IEnumerable<PrintIncident>> GetByStatusAsync(IncidentStatus status);
        Task<PrintIncident> CreateAsync(PrintIncident incident);
        Task<PrintIncident> UpdateAsync(PrintIncident incident);
        Task<PrintIncident?> ResolveAsync(int id, string resolution, int resolvedBy);
        Task<bool> DeleteAsync(int id);
        Task<IncidentStatisticsDto> GetStatisticsAsync(DateTime? start = null, DateTime? end = null);
    }
}
