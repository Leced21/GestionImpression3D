using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class DevisLigneRequest
    {
        public int? PieceId { get; set; }
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int Quantite { get; set; }
        [Range(0, double.MaxValue)]
        public decimal PrixUnitaire { get; set; }
    }
}
