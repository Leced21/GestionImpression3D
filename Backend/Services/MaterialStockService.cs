using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class MaterialStockService : IMaterialStockService
    {
        private readonly IMaterialStockRepository _materialRepository;
        private readonly IAuditLogger _auditLogger;
        private readonly INotificationService _notificationService;
        public MaterialStockService(IMaterialStockRepository materialRepository, IAuditLogger auditLogger, INotificationService notificationService)
        {
            _materialRepository = materialRepository;
            _auditLogger = auditLogger;
            _notificationService = notificationService;
        }
        public async Task<MaterialStockDto?> AddStockAsync(int id, UpdateStockRequest request)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            if (material == null) return null;

            var oldQuantity = material.Quantity;
            material.AddStock(request.Quantity);

            var updated = await _materialRepository.UpdateAsync(material);

            await _auditLogger.LogUpdateAsync(EntityType.MaterialStock, id, "Quantity",
                oldQuantity.ToString(), material.Quantity.ToString());

            return MapToDto(updated);
        }

        public async Task<MaterialStockDto> CreateAsync(CreateMaterialStockRequest request)
        {
            var type = Enum.Parse<MaterialType>(request.Type);
            var unit = Enum.Parse<MaterialUnit>(request.Unit);

            var material = MaterialStock.Create(
                request.Name,
                type,
                request.Brand,
                request.Color,
                request.Quantity,
                unit,
                request.MinThreshold,
                request.MaxThreshold,
                request.UnitPrice,
                request.Location,
                request.Supplier
            );

            var created = await _materialRepository.CreateAsync(material);

            await _auditLogger.LogCreationAsync(EntityType.MaterialStock, created.Id, created.Name);

            return MapToDto(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            if (material == null) return false;

            var result = await _materialRepository.DeleteAsync(id);

            if (result)
            {
                await _auditLogger.LogDeletionAsync(EntityType.MaterialStock, id, material.Name);
            }

            return result;
        }

        public async Task<IEnumerable<MaterialStockDto>> GetAllAsync()
        {
            var materials = await _materialRepository.GetAllAsync();
            return materials.Select(MapToDto);
        }

        public async Task<MaterialStockDto?> GetByIdAsync(int id)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            return material != null ? MapToDto(material) : null;
        }

        public async Task<IEnumerable<MaterialStockDto>> GetLowStockAlertsAsync()
        {
            var materials = await _materialRepository.GetLowStockAsync();
            return materials.Select(MapToDto);
        }

        public async Task<MaterialStatisticsDto> GetStatisticsAsync()
        {
            var materials = await _materialRepository.GetAllAsync();
            var activeMaterials = materials.Where(m => m.IsActive);

            var stats = new MaterialStatisticsDto
            {
                TotalMaterials = activeMaterials.Count(),
                LowStockMaterials = activeMaterials.Count(m => m.IsLowStock()),
                CriticalStockMaterials = activeMaterials.Count(m => m.IsCriticalStock()),
                OutOfStockMaterials = activeMaterials.Count(m => m.Quantity <= 0),
                TotalValue = activeMaterials.Sum(m => m.Quantity * m.UnitPrice)
            };

            foreach (var material in activeMaterials)
            {
                var type = material.Type.ToString();
                stats.ValueByType[type] = stats.ValueByType.GetValueOrDefault(type) + (material.Quantity * material.UnitPrice);
                stats.CountByType[type] = stats.CountByType.GetValueOrDefault(type) + 1;
            }

            return stats;
        }

        public async Task<MaterialStockDto?> RemoveStockAsync(int id, UpdateStockRequest request)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            if (material == null) return null;

            try
            {
                var oldQuantity = material.Quantity;
                material.RemoveStock(request.Quantity);

                var updated = await _materialRepository.UpdateAsync(material);
                if (material.IsLowStock())
                {
                    await _notificationService.SendLowStockAlert(material.Id, material.Name, material.Quantity);
                }

                await _auditLogger.LogUpdateAsync(EntityType.MaterialStock, id, "Quantity",
                    oldQuantity.ToString(), material.Quantity.ToString());

                return MapToDto(updated);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Stock insuffisant: {ex.Message}");
            }
        }

        public async Task<MaterialStockDto?> UpdatePriceAsync(int id, decimal unitPrice)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            if (material == null) return null;

            var oldPrice = material.UnitPrice;
            material.UpdatePrice(unitPrice);

            var updated = await _materialRepository.UpdateAsync(material);

            await _auditLogger.LogUpdateAsync(EntityType.MaterialStock, id, "UnitPrice",
                oldPrice.ToString(), unitPrice.ToString());

            return MapToDto(updated);
        }

        public async Task<MaterialStockDto?> UpdateThresholdsAsync(int id, decimal minThreshold, decimal maxThreshold)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            if (material == null) return null;

            var oldMin = material.MinThreshold;
            var oldMax = material.MaxThreshold;

            material.UpdateThresholds(minThreshold, maxThreshold);

            var updated = await _materialRepository.UpdateAsync(material);

            await _auditLogger.LogUpdateAsync(EntityType.MaterialStock, id, "Thresholds",
                $"{oldMin}/{oldMax}", $"{minThreshold}/{maxThreshold}");

            return MapToDto(updated);
        }

        private static MaterialStockDto MapToDto(MaterialStock material)
        {
            return new MaterialStockDto
            {
                Id = material.Id,
                Name = material.Name,
                Type = material.Type.ToString(),
                Brand = material.Brand,
                Color = material.Color,
                Reference = material.Reference,
                Quantity = material.Quantity,
                Unit = material.Unit.ToString(),
                MinThreshold = material.MinThreshold,
                MaxThreshold = material.MaxThreshold,
                Location = material.Location,
                Supplier = material.Supplier,
                UnitPrice = material.UnitPrice,
                LastRestockedAt = material.LastRestockedAt,
                LastUsedAt = material.LastUsedAt,
                IsLowStock = material.IsLowStock(),
                IsCriticalStock = material.IsCriticalStock(),
                IsActive = material.IsActive,
                Notes = material.Notes
            };
        }
    }
}
