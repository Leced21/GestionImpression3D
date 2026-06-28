namespace Backend.DTOs
{
    public class UpdateThresholdsRequest
    {
        public decimal MinThreshold { get; set; }
        public decimal MaxThreshold { get; set; }
    }
}
