namespace Backend.Models
{
    public class CatalogueItem
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PrixVente { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public string Materiau { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
    }
}
