using Backend.Enums;

namespace Backend.Models
{
    public class MaterialConsumption
    {
        public int Id { get; set; }
        public int MaterialStockId { get; set; }
        public int? PrintJobId { get; set; }
        public int? OrdreFabricationId { get; set; }
        public decimal Quantity { get; set; }
        public MaterialUnit Unit { get; set; } = MaterialUnit.Grams;
        public MaterialConsumptionType Type { get; set; } = MaterialConsumptionType.Production;
        public string? Reason { get; set; }
        public DateTime ConsumedAt { get; set; }
        public int? ConsumedBy { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public MaterialStock MaterialStock { get; set; } = null!;
        public PrintJob? PrintJob { get; set; }
        public OrdreFabrication? OrdreFabrication { get; set; }
        public User? ConsumedByUser { get; set; }
    }
}
