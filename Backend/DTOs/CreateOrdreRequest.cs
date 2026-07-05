using Backend.Enums;

namespace Backend.DTOs
{
    public class CreateOrdreRequest
    {
        public int ProjetId { get; set; }
        public int PieceId { get; set; }
        public int? DevisId { get; set; }
        public int Quantite { get; set; }
        public OrdrePriorite Priorite { get; set; } = OrdrePriorite.Normale;
        public DateTime? DateEcheance { get; set; }
        public string? Notes { get; set; }
    }
}
