namespace Backend.Models
{
    public class DevisLigne
    {
        public int Id { get; set; }
        public int DevisId { get; set; }
        public int? PieceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal Total => Quantite * PrixUnitaire;

        // Navigation
        public Devis Devis { get; set; } = null!;
        public Piece? Piece { get; set; }
    }
}
