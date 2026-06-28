namespace Backend.DTOs
{
    public class AjouterPieceRequest
    {
        public int PieceId { get; set; }
        public int Quantite { get; set; } = 1;
    }
}
