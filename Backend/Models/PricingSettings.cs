namespace Backend.Models
{
    public class PricingSettings
    {
        public int Id { get; set; } = 1;
        public decimal ElectricityPricePerKwh { get; set; } = 0.22m;
        public decimal AveragePrinterPowerKw { get; set; } = 1.5m;
        public decimal PrinterPurchasePrice { get; set; } = 530m;
        public decimal ProductiveLifetimeHours { get; set; } = 5000m;
        public decimal LaborHourlyRate { get; set; } = 1m;
        public decimal WasteRate { get; set; } = 0m;
        public decimal PackagingCostPerPiece { get; set; } = 0m;
        public decimal ConsumablesCostPerPiece { get; set; } = 0m;
        public decimal SellingFeesRate { get; set; } = 0m;
        public decimal TargetGrossMarginRate { get; set; } = 0.5m;
        public decimal VatRate { get; set; } = 0m;
        public decimal PlaPricePerKg { get; set; } = 11m;
        public decimal AbsPricePerKg { get; set; } = 22m;
        public decimal PetgPricePerKg { get; set; } = 10m;
        public decimal TpuPricePerKg { get; set; } = 16.33m;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public decimal MachineAmortizationHourly =>
            ProductiveLifetimeHours > 0 ? PrinterPurchasePrice / ProductiveLifetimeHours : 0m;
    }
}
