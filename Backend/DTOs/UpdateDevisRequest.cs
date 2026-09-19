using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class UpdateDevisRequest
    {
        [Range(1, int.MaxValue)]
        public int ClientId { get; set; }
        public int? ProjetId { get; set; }
        public DateTime DateValidite { get; set; }
        [Range(0, 100)]
        public decimal TVA { get; set; } = 20;
        [StringLength(1000)]
        public string? Notes { get; set; }
        [StringLength(1000)]
        public string? Conditions { get; set; }
        [MinLength(1)]
        public List<DevisLigneRequest> Lignes { get; set; } = new();
    }
}
