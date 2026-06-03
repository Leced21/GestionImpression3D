namespace Backend.Models
{
    public class PieceVersion
    {
        public int Id { get; set; }
        public int PieceId { get; set; }
        public int VersionNumber { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CoutMatiere { get; set; }
        public decimal CoutMachine { get; set; }
        public decimal CoutMainOeuvre { get; set; }
        public decimal PrixVente { get; set; }
        public string StlFileName { get; set; } = string.Empty;
        public string? ChangeLog { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrototype { get; set; }

        // Navigation
        public Piece Piece { get; set; } = null!;
    }
}
