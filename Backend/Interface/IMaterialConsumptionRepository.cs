using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IMaterialConsumptionRepository
    {
        Task<MaterialConsumption?> GetByIdAsync(int id);
        Task<IEnumerable<MaterialConsumption>> GetAllAsync();
        Task<IEnumerable<MaterialConsumption>> GetByMaterialAsync(int materialId);
        Task<IEnumerable<MaterialConsumption>> GetByPrintJobAsync(int printJobId);
        Task<IEnumerable<MaterialConsumption>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<MaterialConsumption> CreateAsync(MaterialConsumption consumption);
        Task<bool> DeleteAsync(int id);
        Task<decimal> GetTotalConsumptionByMaterialAsync(int materialId, DateTime? start = null, DateTime? end = null);
        Task<Dictionary<MaterialConsumptionType, decimal>> GetConsumptionStatisticsAsync(DateTime? start = null, DateTime? end = null);
    }
}
