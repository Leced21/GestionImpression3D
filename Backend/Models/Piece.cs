using System.Text.Json.Serialization;
using Backend.Enums;

namespace Backend.Models
{
    public class Piece
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PieceStatus Statut { get; set; } = PieceStatus.Brouillon;
        public decimal CoutMatiere { get; set; }
        public decimal CoutMachine { get; set; }
        public decimal CoutMainOeuvre { get; set; }
        public decimal PrixVente { get; set; }
        public string StlFileName { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateModification { get; set; }
        // NOUVEAUX ATTRIBUTS POUR LE CATALOGUE
        public string? Categorie { get; set; } = "Mécanique";  // Mécanique, Électronique, Décoration, Outillage
        public string? Materiau { get; set; } = "PLA";         // PLA, PETG, ABS, Résine
        public int Stock { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public bool EstDisponible { get; set; } = true;

        // Champs de la fiche produit (aucune source automatique : renseignés une fois ici,
        // réutilisés à chaque génération de la fiche).
        public string? Couleurs { get; set; }
        public string? CapaciteContenance { get; set; }
        public string? NormesCertifications { get; set; }
        public string? InstructionsUtilisation { get; set; }
        public string? PrecautionsUsage { get; set; }
        public string? PublicCible { get; set; }
        public string? Conditionnement { get; set; }
        public string? DimensionsColis { get; set; }
        public decimal? PoidsColisKg { get; set; }
        public int? MoqUnites { get; set; }
        public int? DelaiLivraisonJours { get; set; }
        public string? PointsForts { get; set; }
        public string? Faq { get; set; }
        public string? TarifsDegressifs { get; set; }

        // Navigation inverse vers les projets contenant cette pièce
        [JsonIgnore]
        public List<ProjetPiece> ProjetPieces { get; set; } = new();

        // Propriétés calculées (non stockées en base)
        public decimal CoutTotal => CoutMatiere + CoutMachine + CoutMainOeuvre;
        public decimal Marge => PrixVente - CoutTotal;
        public decimal MargePourcentage => CoutTotal > 0 ? (Marge / CoutTotal) * 100 : 0;
    }
}
