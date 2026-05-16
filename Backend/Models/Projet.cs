using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Projet
    {
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; } = string.Empty;

        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Statut { get; set; } = "Brouillon"; // Brouillon, EnCours, Termine
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateLivraisonPrevue { get; set; }

        public string ClientNom { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;

        public decimal Budget { get; set; }

        // Navigation
        public List<ProjetPiece> ProjetPieces { get; set; } = new();
    }
}
