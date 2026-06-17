using Backend.DTOs;
using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IPrintIncidentService
    {
        Task<IEnumerable<PrintIncident>> GetAllAsync();
        Task<PrintIncident?> GetByIdAsync(int id);
        Task<IEnumerable<PrintIncident>> GetByPrinterAsync(int printerId);
        Task<IEnumerable<PrintIncident>> GetByPrintJobAsync(int printJobId);
        Task<PrintIncident> CreateAsync(CreateIncidentRequest request);
        Task<PrintIncident?> UpdateStatusAsync(int id, IncidentStatus status);
        Task<PrintIncident?> ResolveAsync(int id, ResolveIncidentRequest request);
        Task<bool> DeleteAsync(int id);
        Task<IncidentStatisticsDto> GetStatisticsAsync(DateTime? start = null, DateTime? end = null);
    }
}
