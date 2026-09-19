using Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class CreateIncidentRequest
    {
        public int? PrintJobId { get; set; }
        public int? PrinterId { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        [EnumDataType(typeof(IncidentSeverity))]
        public IncidentSeverity Severity { get; set; } = IncidentSeverity.Moyenne;
    }
}
