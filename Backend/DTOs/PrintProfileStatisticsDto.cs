namespace Backend.DTOs
{
    public class PrintProfileStatisticsDto
    {
        public int TotalProfiles { get; set; }
        public int ActiveProfiles { get; set; }
        public int DefaultProfiles { get; set; }
        public Dictionary<string, int> CountByMateriau { get; set; } = new();
        public Dictionary<int, int> CountByPrinter { get; set; } = new();
    }
}
