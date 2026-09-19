namespace Backend.Models
{
    public class PricingSettingsDto
    {
        public int Id { get; set; }
        public decimal ElectricityPricePerKwh { get; set; }
        public decimal AveragePrinterPowerKw { get; set; }
        public decimal PrinterPurchasePrice { get; set; }
        public decimal ProductiveLifetimeHours { get; set; }
        public decimal MachineAmortizationHourly { get; set; }
        public decimal LaborHourlyRate { get; set; }
        public decimal WasteRate { get; set; }
        public decimal PackagingCostPerPiece { get; set; }
        public decimal ConsumablesCostPerPiece { get; set; }
        public decimal SellingFeesRate { get; set; }
        public decimal TargetGrossMarginRate { get; set; }
        public decimal VatRate { get; set; }
        public decimal PlaPricePerKg { get; set; }
        public decimal AbsPricePerKg { get; set; }
        public decimal PetgPricePerKg { get; set; }
        public decimal TpuPricePerKg { get; set; }
    }

    public class UpdatePricingSettingsRequest
    {
        public decimal ElectricityPricePerKwh { get; set; }
        public decimal AveragePrinterPowerKw { get; set; }
        public decimal PrinterPurchasePrice { get; set; }
        public decimal ProductiveLifetimeHours { get; set; }
        public decimal LaborHourlyRate { get; set; }
        public decimal WasteRate { get; set; }
        public decimal PackagingCostPerPiece { get; set; }
        public decimal ConsumablesCostPerPiece { get; set; }
        public decimal SellingFeesRate { get; set; }
        public decimal TargetGrossMarginRate { get; set; }
        public decimal VatRate { get; set; }
        public decimal PlaPricePerKg { get; set; }
        public decimal AbsPricePerKg { get; set; }
        public decimal PetgPricePerKg { get; set; }
        public decimal TpuPricePerKg { get; set; }
    }

    public class PieceCostingDto
    {
        public int PieceId { get; set; }
        public string MaterialName { get; set; } = "PLA";
        public decimal QuantityKg { get; set; }
        public decimal FilamentPricePerKg { get; set; }
        public decimal PrintTimeHours { get; set; }
        public decimal ModelingTimeHours { get; set; }
        public decimal PostProcessingTimeHours { get; set; }
        public decimal? RealSalePriceHt { get; set; }
    }

    public class UpdatePieceCostingRequest
    {
        public string MaterialName { get; set; } = "PLA";
        public decimal QuantityKg { get; set; }
        public decimal? FilamentPricePerKg { get; set; }
        public decimal PrintTimeHours { get; set; }
        public decimal ModelingTimeHours { get; set; }
        public decimal PostProcessingTimeHours { get; set; }
        public decimal? RealSalePriceHt { get; set; }
    }

    public class PiecePricingDto
    {
        public int PieceId { get; set; }
        public string PieceName { get; set; } = string.Empty;
        public string PieceReference { get; set; } = string.Empty;
        public PieceCostingDto Costing { get; set; } = new();
        public decimal MaterialCost { get; set; }
        public decimal ElectricityCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal MachineAmortizationCost { get; set; }
        public decimal PackagingCost { get; set; }
        public decimal ConsumablesCost { get; set; }
        public decimal ProductionSubtotal { get; set; }
        public decimal CostWithWaste { get; set; }
        public decimal RecommendedPriceHt { get; set; }
        public decimal RecommendedPriceTtc { get; set; }
        public decimal GrossMargin { get; set; }
        public decimal GrossMarginRate { get; set; }
        public decimal? RealSalePriceHt { get; set; }
        public decimal? RealProfit { get; set; }
        public decimal? RealMarginRate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ProfitabilityDashboardDto
    {
        public int ProductCount { get; set; }
        public decimal TotalRecommendedRevenueHt { get; set; }
        public decimal TotalProductionCost { get; set; }
        public decimal TotalGrossMargin { get; set; }
        public decimal AverageGrossMarginRate { get; set; }
        public List<ProfitabilityDashboardRowDto> Products { get; set; } = new();
    }

    public class ProfitabilityDashboardRowDto
    {
        public int PieceId { get; set; }
        public string PieceName { get; set; } = string.Empty;
        public string PieceReference { get; set; } = string.Empty;
        public decimal CostWithWaste { get; set; }
        public decimal RecommendedPriceHt { get; set; }
        public decimal GrossMarginRate { get; set; }
        public decimal? RealSalePriceHt { get; set; }
        public decimal? RealProfit { get; set; }
        public decimal? RealMarginRate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
