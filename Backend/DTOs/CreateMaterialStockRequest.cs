namespace Backend.DTOs
{
    public class CreateMaterialStockRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "Grams";
        public decimal MinThreshold { get; set; }
        public decimal MaxThreshold { get; set; }
        public string? Location { get; set; }
        public string? Supplier { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
    }
}
