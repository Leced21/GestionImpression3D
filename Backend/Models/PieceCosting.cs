namespace Backend.Models
{
    public class PieceCosting
    {
        public int Id { get; set; }
        public int PieceId { get; set; }
        public string MaterialName { get; set; } = "PLA";
        public decimal QuantityKg { get; set; }
        public decimal? FilamentPricePerKg { get; set; }
        public decimal PrintTimeHours { get; set; }
        public decimal ModelingTimeHours { get; set; }
        public decimal PostProcessingTimeHours { get; set; }
        public decimal? RealSalePriceHt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Piece Piece { get; set; } = null!;
    }
}
