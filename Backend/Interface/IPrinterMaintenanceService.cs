using Backend.DTOs;
using Backend.Models;

namespace Backend.Interface
{
    public interface IPrinterMaintenanceService
    {
        Task<IEnumerable<PrinterMaintenance>> GetAllAsync();
        Task<PrinterMaintenance?> GetByIdAsync(int id);
        Task<IEnumerable<PrinterMaintenance>> GetByPrinterAsync(int printerId);
        Task<IEnumerable<PrinterMaintenance>> GetUpcomingAsync(int days = 7);
        Task<PrinterMaintenance> CreateAsync(CreatePrinterMaintenanceRequest request);
        Task<PrinterMaintenance?> UpdateAsync(int id, UpdatePrinterMaintenanceRequest request);
        Task<PrinterMaintenance?> CompleteAsync(int id, CompleteMaintenanceRequest request);
        Task<PrinterMaintenance?> CancelAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<PrinterMaintenanceStatisticsDto> GetStatisticsAsync(int printerId);
    }
}
