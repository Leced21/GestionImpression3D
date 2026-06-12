using Backend.Enums;

namespace Backend.Models
{
    public class OrdreFabrication
    {
        public int Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public int ProjetId { get; set; }
        public int PieceId { get; set; }
        public int Quantite { get; set; }
        public int QuantiteProduite { get; set; }
        public OrdreStatut Statut { get; set; }= OrdreStatut.EnCours;
        public OrdrePriorite Priorite { get; set; } = OrdrePriorite.Normale;
        public DateTime? DateEcheance { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public Projet Projet { get; set; } = null!;
        public Piece Piece { get; set; } = null!;
        public List<PrintJob> PrintJobs { get; set; } = new();
    }
}
