namespace Backend.DTOs
{
    public class UpdateStockRequest
    {
        public decimal Quantity { get; set; }
        public string? Note { get; set; }
    }
}
