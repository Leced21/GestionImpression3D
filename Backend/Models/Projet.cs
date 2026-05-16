using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Projet
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nom { get; set; } = string.Empty;

        // 1. RETRAIT de [Required] si c'est le backend qui la génère après le POST
        [MaxLength(50)]
        public string Reference { get; set; } = string.Empty;

        // 2. Rendu optionnel (string?) pour éviter la 400 si l'utilisateur ne met rien
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Statut { get; set; } = "Brouillon";

        public DateTime? DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateLivraisonPrevue { get; set; }

        // 3. Rendu optionnel ou à initialiser si non saisis immédiatement
        public string? ClientNom { get; set; }

        [EmailAddress] // S'il y a un email, il doit être valide, mais il peut être nul
        public string? ClientEmail { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }

        // Navigation
        public List<ProjetPiece> ProjetPieces { get; set; } = new();
    }
}