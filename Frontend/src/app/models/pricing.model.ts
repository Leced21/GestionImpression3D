export interface PricingSettings {
  id: number;
  electricityPricePerKwh: number;
  averagePrinterPowerKw: number;
  printerPurchasePrice: number;
  productiveLifetimeHours: number;
  machineAmortizationHourly: number;
  laborHourlyRate: number;
  wasteRate: number;
  packagingCostPerPiece: number;
  consumablesCostPerPiece: number;
  sellingFeesRate: number;
  targetGrossMarginRate: number;
  vatRate: number;
  plaPricePerKg: number;
  absPricePerKg: number;
  petgPricePerKg: number;
  tpuPricePerKg: number;
}

export interface PieceCosting {
  pieceId: number;
  materialName: string;
  quantityKg: number;
  filamentPricePerKg: number;
  printTimeHours: number;
  modelingTimeHours: number;
  postProcessingTimeHours: number;
  realSalePriceHt?: number | null;
}

export interface PiecePricing {
  pieceId: number;
  pieceName: string;
  pieceReference: string;
  costing: PieceCosting;
  materialCost: number;
  electricityCost: number;
  laborCost: number;
  machineAmortizationCost: number;
  packagingCost: number;
  consumablesCost: number;
  productionSubtotal: number;
  costWithWaste: number;
  recommendedPriceHt: number;
  recommendedPriceTtc: number;
  grossMargin: number;
  grossMarginRate: number;
  realSalePriceHt?: number | null;
  realProfit?: number | null;
  realMarginRate?: number | null;
  status: string;
}

export interface ProfitabilityDashboard {
  productCount: number;
  totalRecommendedRevenueHt: number;
  totalProductionCost: number;
  totalGrossMargin: number;
  averageGrossMarginRate: number;
  products: ProfitabilityDashboardRow[];
}

export interface ProfitabilityDashboardRow {
  pieceId: number;
  pieceName: string;
  pieceReference: string;
  costWithWaste: number;
  recommendedPriceHt: number;
  grossMarginRate: number;
  realSalePriceHt?: number | null;
  realProfit?: number | null;
  realMarginRate?: number | null;
  status: string;
}
