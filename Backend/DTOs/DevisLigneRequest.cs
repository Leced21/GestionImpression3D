namespace Backend.DTOs
{
    public class DevisLigneRequest
    {
        public int? PieceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
    }
}
