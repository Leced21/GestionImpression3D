using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class CompleteMaintenanceRequest
    {
        [StringLength(500)]
        public string? Notes { get; set; }
        [StringLength(100)]
        public string? PerformedBy { get; set; }
    }
}
