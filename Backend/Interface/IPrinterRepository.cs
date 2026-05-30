using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IPrinterRepository
    {
        Task<Printer?> GetByIdAsync(int id);
        Task<IEnumerable<Printer>> GetAllAsync();
        Task<IEnumerable<Printer>> GetByStatusAsync(PrinterStatus status);
        Task<Printer> CreateAsync(Printer printer);
        Task<Printer> UpdateAsync(Printer printer);
        Task<bool> DeleteAsync(int id);
        Task<int> GetAvailableCountAsync();
    }
}
