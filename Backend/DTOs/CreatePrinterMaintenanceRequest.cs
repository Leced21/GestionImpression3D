using Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class CreatePrinterMaintenanceRequest
    {
        [Range(1, int.MaxValue)]
        public int PrinterId { get; set; }
        [EnumDataType(typeof(MaintenanceType))]
        public MaintenanceType Type { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        [Range(1, int.MaxValue)]
        public int DurationMinutes { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
