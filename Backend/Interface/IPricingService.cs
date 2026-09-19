using Backend.Models;

namespace Backend.Interface
{
    public interface IPricingService
    {
        Task<PricingSettingsDto> GetSettingsAsync();
        Task<PricingSettingsDto> UpdateSettingsAsync(UpdatePricingSettingsRequest request);
        Task<PiecePricingDto?> GetPiecePricingAsync(int pieceId);
        Task<PiecePricingDto?> UpdatePieceCostingAsync(int pieceId, UpdatePieceCostingRequest request);
        Task<ProfitabilityDashboardDto> GetDashboardAsync();
    }
}
