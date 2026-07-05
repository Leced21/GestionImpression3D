namespace Backend.Models
{
    public class FactureLigne
    {
        public int Id { get; set; }
        public int FactureId { get; set; }
        public int? PieceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal Total => Quantite * PrixUnitaire;

        // Navigation
        public Facture Facture { get; set; } = null!;
        public Piece? Piece { get; set; }
    }
}
