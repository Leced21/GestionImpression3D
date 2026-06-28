namespace Backend.DTOs
{
    public class MaterialStatisticsDto
    {
        public int TotalMaterials { get; set; }
        public int LowStockMaterials { get; set; }
        public int CriticalStockMaterials { get; set; }
        public int OutOfStockMaterials { get; set; }
        public decimal TotalValue { get; set; }
        public Dictionary<string, decimal> ValueByType { get; set; } = new();
        public Dictionary<string, int> CountByType { get; set; } = new();
    }
}
