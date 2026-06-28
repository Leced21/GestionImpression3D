namespace Backend.DTOs
{
    public class DevisStatisticsDto
    {
        public int TotalDevis { get; set; }
        public int BrouillonCount { get; set; }
        public int EnvoyesCount { get; set; }
        public int AcceptesCount { get; set; }
        public int RefusesCount { get; set; }
        public int ExpiresCount { get; set; }
        public decimal TotalAmountAccepted { get; set; }
        public decimal AverageAmount { get; set; }
    }
}
