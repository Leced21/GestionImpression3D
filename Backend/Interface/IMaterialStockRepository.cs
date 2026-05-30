using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IMaterialStockRepository
    {
        Task<MaterialStock?> GetByIdAsync(int id);
        Task<IEnumerable<MaterialStock>> GetAllAsync();
        Task<IEnumerable<MaterialStock>> GetLowStockAsync();
        Task<IEnumerable<MaterialStock>> GetByTypeAsync(MaterialType type);
        Task<MaterialStock> CreateAsync(MaterialStock material);
        Task<MaterialStock> UpdateAsync(MaterialStock material);
        Task<bool> DeleteAsync(int id);
        Task<decimal> GetTotalValueAsync();
    }
}
