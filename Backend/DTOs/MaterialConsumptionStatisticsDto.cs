namespace Backend.DTOs
{
    public class MaterialConsumptionStatisticsDto
    {
        public decimal TotalConsumption { get; set; }
        public decimal ProductionConsumption { get; set; }
        public decimal WasteConsumption { get; set; }
        public decimal TestConsumption { get; set; }
        public decimal MaintenanceConsumption { get; set; }
        public Dictionary<string, decimal> ConsumptionByMaterial { get; set; } = new();
        public Dictionary<string, decimal> ConsumptionByMonth { get; set; } = new();
    }
}
