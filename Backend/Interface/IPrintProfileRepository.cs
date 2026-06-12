using Backend.Models;

namespace Backend.Interface
{
    public interface IPrintProfileRepository
    {
        Task<PrintProfile?> GetByIdAsync(int id);
        Task<IEnumerable<PrintProfile>> GetAllAsync();
        Task<IEnumerable<PrintProfile>> GetByPrinterAsync(int printerId);
        Task<IEnumerable<PrintProfile>> GetByMateriauAsync(string materiau);
        Task<PrintProfile?> GetDefaultForPrinterAsync(int printerId);
        Task<PrintProfile> CreateAsync(PrintProfile profile);
        Task<PrintProfile> UpdateAsync(PrintProfile profile);
        Task<bool> DeleteAsync(int id);
        Task<bool> SetDefaultAsync(int printerId, int profileId);
    }
}
