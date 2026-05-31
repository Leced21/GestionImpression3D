namespace Backend.Interface
{
    public interface IDashboardService
    {
        Task<object> GetGlobalStatsAsync();
        Task<object> GetProductionTrendAsync(int days);
        Task<object> GetMaterialConsumptionAsync(int days);
        Task<object> GetPrintersActivityAsync();
    }
}
