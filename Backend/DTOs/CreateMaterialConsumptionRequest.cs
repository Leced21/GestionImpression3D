using Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class CreateMaterialConsumptionRequest
    {
        [Range(1, int.MaxValue)]
        public int MaterialStockId { get; set; }
        public int? PrintJobId { get; set; }
        public int? OrdreFabricationId { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }
        [EnumDataType(typeof(MaterialConsumptionType))]
        public MaterialConsumptionType Type { get; set; } = MaterialConsumptionType.Production;
        [StringLength(200)]
        public string? Reason { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
