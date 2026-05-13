namespace Backend.Models
{
    public class Piece
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Statut { get; set; } = "Brouillon";
        public decimal CoutMatiere { get; set; }
        public decimal CoutMachine { get; set; }
        public decimal CoutMainOeuvre { get; set; }
        public decimal PrixVente { get; set; }
        public string StlFileName { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateModification { get; set; }

        // Propriétés calculées (non stockées en base)
        public decimal CoutTotal => CoutMatiere + CoutMachine + CoutMainOeuvre;
        public decimal Marge => PrixVente - CoutTotal;
        public decimal MargePourcentage => CoutTotal > 0 ? (Marge / CoutTotal) * 100 : 0;
    }
}
