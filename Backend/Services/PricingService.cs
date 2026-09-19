using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class PricingService : IPricingService
    {
        private readonly AppDbContext _context;

        public PricingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PricingSettingsDto> GetSettingsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            return MapSettings(settings);
        }

        public async Task<PricingSettingsDto> UpdateSettingsAsync(UpdatePricingSettingsRequest request)
        {
            ValidateSettings(request);

            var settings = await GetOrCreateSettingsAsync();
            settings.ElectricityPricePerKwh = request.ElectricityPricePerKwh;
            settings.AveragePrinterPowerKw = request.AveragePrinterPowerKw;
            settings.PrinterPurchasePrice = request.PrinterPurchasePrice;
            settings.ProductiveLifetimeHours = request.ProductiveLifetimeHours;
            settings.LaborHourlyRate = request.LaborHourlyRate;
            settings.WasteRate = request.WasteRate;
            settings.PackagingCostPerPiece = request.PackagingCostPerPiece;
            settings.ConsumablesCostPerPiece = request.ConsumablesCostPerPiece;
            settings.SellingFeesRate = request.SellingFeesRate;
            settings.TargetGrossMarginRate = request.TargetGrossMarginRate;
            settings.VatRate = request.VatRate;
            settings.PlaPricePerKg = request.PlaPricePerKg;
            settings.AbsPricePerKg = request.AbsPricePerKg;
            settings.PetgPricePerKg = request.PetgPricePerKg;
            settings.TpuPricePerKg = request.TpuPricePerKg;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapSettings(settings);
        }

        public async Task<PiecePricingDto?> GetPiecePricingAsync(int pieceId)
        {
            var piece = await _context.Pieces.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pieceId);
            if (piece == null)
            {
                return null;
            }

            var settings = await GetOrCreateSettingsAsync();
            var costing = await _context.PieceCostings.AsNoTracking().FirstOrDefaultAsync(c => c.PieceId == pieceId)
                ?? BuildDefaultCosting(piece, settings);

            return Calculate(piece, costing, settings);
        }

        public async Task<PiecePricingDto?> UpdatePieceCostingAsync(int pieceId, UpdatePieceCostingRequest request)
        {
            ValidatePieceCosting(request);

            var piece = await _context.Pieces.FirstOrDefaultAsync(p => p.Id == pieceId);
            if (piece == null)
            {
                return null;
            }

            var costing = await _context.PieceCostings.FirstOrDefaultAsync(c => c.PieceId == pieceId);
            if (costing == null)
            {
                costing = new PieceCosting
                {
                    PieceId = pieceId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PieceCostings.Add(costing);
            }

            costing.MaterialName = NormalizeMaterialName(request.MaterialName);
            costing.QuantityKg = request.QuantityKg;
            costing.FilamentPricePerKg = request.FilamentPricePerKg;
            costing.PrintTimeHours = request.PrintTimeHours;
            costing.ModelingTimeHours = request.ModelingTimeHours;
            costing.PostProcessingTimeHours = request.PostProcessingTimeHours;
            costing.RealSalePriceHt = request.RealSalePriceHt;
            costing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var settings = await GetOrCreateSettingsAsync();
            return Calculate(piece, costing, settings);
        }

        public async Task<ProfitabilityDashboardDto> GetDashboardAsync()
        {
            var pieces = await _context.Pieces.AsNoTracking().OrderBy(p => p.Nom).ToListAsync();
            var costings = await _context.PieceCostings.AsNoTracking().ToDictionaryAsync(c => c.PieceId);
            var settings = await GetOrCreateSettingsAsync();

            var rows = pieces
                .Select(piece =>
                {
                    var costing = costings.TryGetValue(piece.Id, out var existing)
                        ? existing
                        : BuildDefaultCosting(piece, settings);

                    var pricing = Calculate(piece, costing, settings);
                    return new ProfitabilityDashboardRowDto
                    {
                        PieceId = pricing.PieceId,
                        PieceName = pricing.PieceName,
                        PieceReference = pricing.PieceReference,
                        CostWithWaste = pricing.CostWithWaste,
                        RecommendedPriceHt = pricing.RecommendedPriceHt,
                        GrossMarginRate = pricing.GrossMarginRate,
                        RealSalePriceHt = pricing.RealSalePriceHt,
                        RealProfit = pricing.RealProfit,
                        RealMarginRate = pricing.RealMarginRate,
                        Status = pricing.Status
                    };
                })
                .ToList();

            return new ProfitabilityDashboardDto
            {
                ProductCount = rows.Count,
                TotalRecommendedRevenueHt = Round(rows.Sum(r => r.RecommendedPriceHt)),
                TotalProductionCost = Round(rows.Sum(r => r.CostWithWaste)),
                TotalGrossMargin = Round(rows.Sum(r => r.RecommendedPriceHt - r.CostWithWaste)),
                AverageGrossMarginRate = rows.Count > 0 ? RoundRate(rows.Average(r => r.GrossMarginRate)) : 0m,
                Products = rows
            };
        }

        private async Task<PricingSettings> GetOrCreateSettingsAsync()
        {
            var settings = await _context.PricingSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (settings != null)
            {
                return settings;
            }

            settings = new PricingSettings { Id = 1, CreatedAt = DateTime.UtcNow };
            _context.PricingSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        private static PiecePricingDto Calculate(Piece piece, PieceCosting costing, PricingSettings settings)
        {
            var materialName = NormalizeMaterialName(costing.MaterialName);
            var filamentPrice = costing.FilamentPricePerKg ?? GetDefaultMaterialPrice(settings, materialName);
            var materialCost = costing.QuantityKg * filamentPrice;
            var electricityCost = costing.PrintTimeHours * settings.ElectricityPricePerKwh * settings.AveragePrinterPowerKw;
            var laborCost = (costing.ModelingTimeHours + costing.PostProcessingTimeHours) * settings.LaborHourlyRate;
            var machineCost = costing.PrintTimeHours * settings.MachineAmortizationHourly;
            var subtotal = materialCost
                + electricityCost
                + laborCost
                + machineCost
                + settings.PackagingCostPerPiece
                + settings.ConsumablesCostPerPiece;

            var costWithWaste = settings.WasteRate >= 1m
                ? subtotal
                : subtotal / (1m - settings.WasteRate);

            var denominator = 1m - settings.TargetGrossMarginRate - settings.SellingFeesRate;
            var recommendedPriceHt = denominator > 0m ? costWithWaste / denominator : costWithWaste;
            var recommendedPriceTtc = recommendedPriceHt * (1m + settings.VatRate);
            var grossMargin = recommendedPriceHt - costWithWaste;
            var realSalePrice = costing.RealSalePriceHt ?? (piece.PrixVente > 0m ? piece.PrixVente : null);
            decimal? realProfit = realSalePrice.HasValue ? realSalePrice.Value - costWithWaste : null;
            decimal? realMarginRate = realSalePrice is > 0m ? realProfit / realSalePrice.Value : null;

            return new PiecePricingDto
            {
                PieceId = piece.Id,
                PieceName = piece.Nom,
                PieceReference = piece.Reference,
                Costing = new PieceCostingDto
                {
                    PieceId = piece.Id,
                    MaterialName = materialName,
                    QuantityKg = RoundQuantity(costing.QuantityKg),
                    FilamentPricePerKg = Round(filamentPrice),
                    PrintTimeHours = RoundQuantity(costing.PrintTimeHours),
                    ModelingTimeHours = RoundQuantity(costing.ModelingTimeHours),
                    PostProcessingTimeHours = RoundQuantity(costing.PostProcessingTimeHours),
                    RealSalePriceHt = realSalePrice.HasValue ? Round(realSalePrice.Value) : null
                },
                MaterialCost = Round(materialCost),
                ElectricityCost = Round(electricityCost),
                LaborCost = Round(laborCost),
                MachineAmortizationCost = Round(machineCost),
                PackagingCost = Round(settings.PackagingCostPerPiece),
                ConsumablesCost = Round(settings.ConsumablesCostPerPiece),
                ProductionSubtotal = Round(subtotal),
                CostWithWaste = Round(costWithWaste),
                RecommendedPriceHt = Round(recommendedPriceHt),
                RecommendedPriceTtc = Round(recommendedPriceTtc),
                GrossMargin = Round(grossMargin),
                GrossMarginRate = recommendedPriceHt > 0m ? RoundRate(grossMargin / recommendedPriceHt) : 0m,
                RealSalePriceHt = realSalePrice.HasValue ? Round(realSalePrice.Value) : null,
                RealProfit = realProfit.HasValue ? Round(realProfit.Value) : null,
                RealMarginRate = realMarginRate.HasValue ? RoundRate(realMarginRate.Value) : null,
                Status = GetStatus(realSalePrice, realProfit, realMarginRate, settings.TargetGrossMarginRate)
            };
        }

        private static PieceCosting BuildDefaultCosting(Piece piece, PricingSettings settings)
        {
            var materialName = NormalizeMaterialName(piece.Materiau.ToString());
            var filamentPrice = GetDefaultMaterialPrice(settings, materialName);
            var quantityKg = filamentPrice > 0m && piece.CoutMatiere > 0m ? piece.CoutMatiere / filamentPrice : 0m;
            var machineHourly = settings.MachineAmortizationHourly > 0m ? settings.MachineAmortizationHourly : 1m;
            var printTimeHours = piece.CoutMachine > 0m ? piece.CoutMachine / machineHourly : 0m;
            var modelingHours = settings.LaborHourlyRate > 0m && piece.CoutMainOeuvre > 0m
                ? piece.CoutMainOeuvre / settings.LaborHourlyRate
                : 0m;

            return new PieceCosting
            {
                PieceId = piece.Id,
                MaterialName = materialName,
                QuantityKg = quantityKg,
                FilamentPricePerKg = filamentPrice,
                PrintTimeHours = printTimeHours,
                ModelingTimeHours = modelingHours,
                PostProcessingTimeHours = 0m,
                RealSalePriceHt = piece.PrixVente > 0m ? piece.PrixVente : null
            };
        }

        private static PricingSettingsDto MapSettings(PricingSettings settings)
        {
            return new PricingSettingsDto
            {
                Id = settings.Id,
                ElectricityPricePerKwh = settings.ElectricityPricePerKwh,
                AveragePrinterPowerKw = settings.AveragePrinterPowerKw,
                PrinterPurchasePrice = settings.PrinterPurchasePrice,
                ProductiveLifetimeHours = settings.ProductiveLifetimeHours,
                MachineAmortizationHourly = RoundQuantity(settings.MachineAmortizationHourly),
                LaborHourlyRate = settings.LaborHourlyRate,
                WasteRate = settings.WasteRate,
                PackagingCostPerPiece = settings.PackagingCostPerPiece,
                ConsumablesCostPerPiece = settings.ConsumablesCostPerPiece,
                SellingFeesRate = settings.SellingFeesRate,
                TargetGrossMarginRate = settings.TargetGrossMarginRate,
                VatRate = settings.VatRate,
                PlaPricePerKg = settings.PlaPricePerKg,
                AbsPricePerKg = settings.AbsPricePerKg,
                PetgPricePerKg = settings.PetgPricePerKg,
                TpuPricePerKg = settings.TpuPricePerKg
            };
        }

        private static decimal GetDefaultMaterialPrice(PricingSettings settings, string materialName)
        {
            return materialName.ToUpperInvariant() switch
            {
                "ABS" => settings.AbsPricePerKg,
                "PETG" => settings.PetgPricePerKg,
                "TPU" => settings.TpuPricePerKg,
                _ => settings.PlaPricePerKg
            };
        }

        private static string NormalizeMaterialName(string? materialName)
        {
            var value = string.IsNullOrWhiteSpace(materialName) ? "PLA" : materialName.Trim().ToUpperInvariant();
            return value == "RESINE" || value == "RESIN" ? "PLA" : value;
        }

        private static string GetStatus(decimal? realSalePrice, decimal? realProfit, decimal? realMarginRate, decimal targetMarginRate)
        {
            if (!realSalePrice.HasValue)
            {
                return "Prix reel a saisir";
            }

            if (realProfit < 0m)
            {
                return "A perte";
            }

            return realMarginRate < targetMarginRate ? "Sous cible" : "Rentable";
        }

        private static void ValidateSettings(UpdatePricingSettingsRequest request)
        {
            if (request.ProductiveLifetimeHours <= 0m)
            {
                throw new ArgumentException("La duree de vie productive doit etre superieure a 0.");
            }

            if (request.TargetGrossMarginRate + request.SellingFeesRate >= 1m)
            {
                throw new ArgumentException("La marge cible et les frais de vente doivent rester inferieurs a 100%.");
            }

            if (request.ElectricityPricePerKwh < 0m
                || request.AveragePrinterPowerKw < 0m
                || request.PrinterPurchasePrice < 0m
                || request.LaborHourlyRate < 0m
                || request.WasteRate < 0m
                || request.PackagingCostPerPiece < 0m
                || request.ConsumablesCostPerPiece < 0m
                || request.SellingFeesRate < 0m
                || request.TargetGrossMarginRate < 0m
                || request.VatRate < 0m
                || request.PlaPricePerKg < 0m
                || request.AbsPricePerKg < 0m
                || request.PetgPricePerKg < 0m
                || request.TpuPricePerKg < 0m)
            {
                throw new ArgumentException("Les valeurs de tarification ne peuvent pas etre negatives.");
            }
        }

        private static void ValidatePieceCosting(UpdatePieceCostingRequest request)
        {
            if (request.QuantityKg < 0m
                || request.FilamentPricePerKg < 0m
                || request.PrintTimeHours < 0m
                || request.ModelingTimeHours < 0m
                || request.PostProcessingTimeHours < 0m
                || request.RealSalePriceHt < 0m)
            {
                throw new ArgumentException("Les valeurs de calcul d'une piece ne peuvent pas etre negatives.");
            }
        }

        private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
        private static decimal RoundRate(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);
        private static decimal RoundQuantity(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);
    }
}
