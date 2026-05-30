using Backend.DTOs;

namespace Backend.Interface
{
    public interface IPrinterService
    {
        Task<IEnumerable<PrinterDto>> GetAllAsync();
        Task<PrinterDto?> GetByIdAsync(int id);
        Task<PrinterDto> CreateAsync(CreatePrinterRequest request);
        Task<PrinterDto?> UpdateAsync(int id, UpdatePrinterRequest request);
        Task<PrinterDto?> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);
        Task<PrinterStatisticsDto> GetStatisticsAsync();
    }
}
