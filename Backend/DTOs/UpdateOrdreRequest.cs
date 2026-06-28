using Backend.Enums;

namespace Backend.DTOs
{
    public class UpdateOrdreRequest
    {
        public int Quantite { get; set; }
        public OrdrePriorite Priorite { get; set; } = OrdrePriorite.Normale;
        public DateTime? DateEcheance { get; set; }
        public string? Notes { get; set; }
    }
}
