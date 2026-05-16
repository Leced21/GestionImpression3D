using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class ProjetPiece
    {
        public int Id { get; set; }
        public int ProjetId { get; set; }
        public int PieceId { get; set; }
        public int Quantite { get; set; } = 1;
        public DateTime DateAjout { get; set; } = DateTime.Now;

        // Navigation
        [JsonIgnore]
        public Projet Projet { get; set; } = null!;
        public Piece Piece { get; set; } = null!;
    }
}
