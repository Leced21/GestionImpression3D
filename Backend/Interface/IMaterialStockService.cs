using Backend.DTOs;

namespace Backend.Interface
{
    public interface IMaterialStockService
    {
        Task<IEnumerable<MaterialStockDto>> GetAllAsync();
        Task<MaterialStockDto?> GetByIdAsync(int id);
        Task<MaterialStockDto> CreateAsync(CreateMaterialStockRequest request);
        Task<MaterialStockDto?> AddStockAsync(int id, UpdateStockRequest request);
        Task<MaterialStockDto?> RemoveStockAsync(int id, UpdateStockRequest request);
        Task<MaterialStockDto?> UpdateThresholdsAsync(int id, decimal minThreshold, decimal maxThreshold);
        Task<MaterialStockDto?> UpdatePriceAsync(int id, decimal unitPrice);
        Task<bool> DeleteAsync(int id);
        Task<MaterialStatisticsDto> GetStatisticsAsync();
        Task<IEnumerable<MaterialStockDto>> GetLowStockAlertsAsync();
    }
}
