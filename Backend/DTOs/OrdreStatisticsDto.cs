namespace Backend.DTOs
{
    public class OrdreStatisticsDto
    {
        public int TotalOrdres { get; set; }
        public int EnAttente { get; set; }
        public int EnCours { get; set; }
        public int Termines { get; set; }
        public int Annules { get; set; }
        public int EnRetard { get; set; }
        public int QuantiteTotale { get; set; }
        public int QuantiteProduite { get; set; }
        public decimal TauxAvancement { get; set; }
    }
}
