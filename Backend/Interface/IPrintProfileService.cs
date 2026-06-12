using Backend.DTOs;
using Backend.Models;

namespace Backend.Interface
{
    public interface IPrintProfileService
    {
        Task<IEnumerable<PrintProfile>> GetAllAsync();
        Task<PrintProfile?> GetByIdAsync(int id);
        Task<IEnumerable<PrintProfile>> GetByPrinterAsync(int printerId);
        Task<IEnumerable<PrintProfile>> GetByMateriauAsync(string materiau);
        Task<PrintProfile?> GetDefaultForPrinterAsync(int printerId);
        Task<PrintProfile> CreateAsync(CreatePrintProfileRequest request);
        Task<PrintProfile?> UpdateAsync(int id, UpdatePrintProfileRequest request);
        Task<PrintProfile?> SetDefaultAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<PrintProfile?> DuplicateAsync(int id, string newName);
        Task<PrintProfileStatisticsDto> GetStatisticsAsync();
    }
}
