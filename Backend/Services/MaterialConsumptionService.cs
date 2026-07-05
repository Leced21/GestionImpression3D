using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class MaterialConsumptionService : IMaterialConsumptionService
    {
        private readonly IMaterialConsumptionRepository _consumptionRepository;
        private readonly IMaterialStockRepository _materialRepository;
        private readonly IAuditLogger _auditLogger;
        private readonly ICurrentUserService _currentUser;
        public MaterialConsumptionService(IMaterialConsumptionRepository consumptionRepository, IMaterialStockRepository materialRepository, IAuditLogger auditLogger, ICurrentUserService currentUser)
        {
            _consumptionRepository = consumptionRepository;
            _materialRepository = materialRepository;
            _auditLogger = auditLogger;
            _currentUser = currentUser;
        }
        public async Task<MaterialConsumption> CreateAsync(CreateMaterialConsumptionRequest request)
        {
            if (request.Quantity <= 0)
                throw new InvalidOperationException("La quantité consommée doit être positive");

            var material = await _materialRepository.GetByIdAsync(request.MaterialStockId);
            if (material == null)
                throw new InvalidOperationException("Matériau non trouvé");

            if (material.Quantity < request.Quantity)
                throw new InvalidOperationException($"Stock insuffisant. Disponible: {material.Quantity} {material.Unit}");

            var consumption = new MaterialConsumption
            {
                MaterialStockId = request.MaterialStockId,
                PrintJobId = request.PrintJobId,
                OrdreFabricationId = request.OrdreFabricationId,
                Quantity = request.Quantity,
                Unit = material.Unit,
                Type = request.Type,
                Reason = request.Reason,
                Notes = request.Notes,
                ConsumedAt = DateTime.UtcNow,
                ConsumedBy = _currentUser.UserId
            };

            // Mettre à jour le stock
            material.RemoveStock(request.Quantity);
            await _materialRepository.UpdateAsync(material);

            var created = await _consumptionRepository.CreateAsync(consumption);

            await _auditLogger.LogCreationAsync(EntityType.MaterialConsumption, created.Id,
                $"Consommation de {request.Quantity} {material.Unit} de {material.Name}");

            return created;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var consumption = await _consumptionRepository.GetByIdAsync(id);
            if (consumption == null) return false;

            if (consumption.MaterialStock != null)
            {
                consumption.MaterialStock.AddStock(consumption.Quantity);
                await _materialRepository.UpdateAsync(consumption.MaterialStock);
            }

            var result = await _consumptionRepository.DeleteAsync(id);

            if (result)
                await _auditLogger.LogDeletionAsync(EntityType.MaterialConsumption, id, "Consommation supprimée et stock restauré");

            return result;
        }

        public async Task<IEnumerable<MaterialConsumption>> GetAllAsync()
        {
            return await _consumptionRepository.GetAllAsync();
        }

        public async Task<MaterialConsumption?> GetByIdAsync(int id)
        {
            return await _consumptionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<MaterialConsumption>> GetByMaterialAsync(int materialId)
        {
            return await _consumptionRepository.GetByMaterialAsync(materialId);
        }

        public async Task<IEnumerable<MaterialConsumption>> GetByPrintJobAsync(int printJobId)
        {
            return await _consumptionRepository.GetByPrintJobAsync(printJobId);
        }

        public async Task<MaterialConsumptionStatisticsDto> GetStatisticsAsync(DateTime? start = null, DateTime? end = null)
        {
            var consumptions = await _consumptionRepository.GetAllAsync();
            var filtered = consumptions.AsEnumerable();

            if (start.HasValue)
                filtered = filtered.Where(c => c.ConsumedAt >= start.Value);
            if (end.HasValue)
                filtered = filtered.Where(c => c.ConsumedAt <= end.Value);

            var list = filtered.ToList();

            var stats = new MaterialConsumptionStatisticsDto
            {
                TotalConsumption = list.Sum(c => c.Quantity),
                ProductionConsumption = list.Where(c => c.Type == MaterialConsumptionType.Production).Sum(c => c.Quantity),
                WasteConsumption = list.Where(c => c.Type == MaterialConsumptionType.Waste || c.Type == MaterialConsumptionType.Perte).Sum(c => c.Quantity),
                TestConsumption = list.Where(c => c.Type == MaterialConsumptionType.Test).Sum(c => c.Quantity),
                MaintenanceConsumption = list.Where(c => c.Type == MaterialConsumptionType.Maintenance).Sum(c => c.Quantity),
                ConsumptionByMaterial = list.GroupBy(c => c.MaterialStock?.Name ?? "Inconnu")
                                            .ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity)),
                ConsumptionByMonth = list.GroupBy(c => c.ConsumedAt.ToString("yyyy-MM"))
                                         .ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity))
            };

            return stats;
        }

        public async Task<decimal> GetTotalConsumptionAsync(int materialId)
        {
            return await _consumptionRepository.GetTotalConsumptionByMaterialAsync(materialId);
        }
    }
}
