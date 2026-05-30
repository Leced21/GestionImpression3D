namespace Backend.DTOs
{
    public class MaterialStockDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TypeLabel => GetTypeLabel();
        public string Brand { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string UnitLabel => GetUnitLabel();
        public decimal MinThreshold { get; set; }
        public decimal MaxThreshold { get; set; }
        public string? Location { get; set; }
        public string? Supplier { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue => Quantity * UnitPrice;
        public DateTime? LastRestockedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsCriticalStock { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public int UsageCount { get; set; }

        private string GetTypeLabel() => Type switch
        {
            "PLA" => "PLA",
            "PETG" => "PETG",
            "ABS" => "ABS",
            "TPU" => "TPU",
            "Nylon" => "Nylon",
            "Resin" => "Résine",
            _ => "Autre"
        };

        private string GetUnitLabel() => Unit switch
        {
            "Grams" => "g",
            "Kilograms" => "kg",
            "Meters" => "m",
            "Liters" => "L",
            _ => "g"
        };
    }
}

