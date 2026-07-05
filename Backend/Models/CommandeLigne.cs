namespace Backend.Models
{
    public class CommandeLigne
    {
        public int Id { get; set; }
        public int CommandeId { get; set; }
        public int PieceId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal Total => Quantite * PrixUnitaire;

        public Commande Commande { get; set; } = null!;
        public Piece Piece { get; set; } = null!;
    }
}
