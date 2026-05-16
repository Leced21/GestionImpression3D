namespace Backend.Models
{
    public class CommandeItem
    {
        public int PieceId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
    }
}
