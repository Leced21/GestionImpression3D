using Backend.DTOs;
using Backend.Models;

namespace Backend.Interface
{
    public interface IMaterialConsumptionService
    {
        Task<IEnumerable<MaterialConsumption>> GetAllAsync();
        Task<MaterialConsumption?> GetByIdAsync(int id);
        Task<IEnumerable<MaterialConsumption>> GetByMaterialAsync(int materialId);
        Task<IEnumerable<MaterialConsumption>> GetByPrintJobAsync(int printJobId);
        Task<MaterialConsumption> CreateAsync(CreateMaterialConsumptionRequest request);
        Task<bool> DeleteAsync(int id);
        Task<MaterialConsumptionStatisticsDto> GetStatisticsAsync(DateTime? start = null, DateTime? end = null);
        Task<decimal> GetTotalConsumptionAsync(int materialId);
    }
}
