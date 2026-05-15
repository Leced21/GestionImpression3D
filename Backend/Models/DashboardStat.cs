namespace Backend.Models
{
    public class DashboardStat
    {
        public int Id { get; set; }
        public int TotalPieces { get; set; }
        public int EnConception { get; set; }
        public int EnPrototypage { get; set; }
        public int EnProduction { get; set; }
        public int Commercialisables { get; set; }
        public decimal ChiffreAffaires { get; set; }
    }
}
