using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class ResolveIncidentRequest
    {
        [Required]
        [StringLength(500)]
        public string Resolution { get; set; } = string.Empty;
    }
}
