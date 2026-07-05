using Backend.DTOs;
using Backend.Models;

namespace Backend.Interface
{
    public interface IPrinterMaintenanceRepository
    {
        Task<PrinterMaintenance?> GetByIdAsync(int id);
        Task<IEnumerable<PrinterMaintenance>> GetAllAsync();
        Task<IEnumerable<PrinterMaintenance>> GetByPrinterAsync(int printerId);
        Task<IEnumerable<PrinterMaintenance>> GetUpcomingAsync(int days = 7);
        Task<PrinterMaintenance> CreateAsync(PrinterMaintenance maintenance);
        Task<PrinterMaintenance> UpdateAsync(PrinterMaintenance maintenance);
        Task<bool> DeleteAsync(int id);
        Task<PrinterMaintenance?> CompleteAsync(int id, string? notes = null, string? performedBy = null);
        Task<PrinterMaintenanceStatisticsDto> GetStatisticsAsync(int printerId);
    }
}
