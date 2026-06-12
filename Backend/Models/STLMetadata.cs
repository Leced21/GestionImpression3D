namespace Backend.Models
{
    public class STLMetadata
    {
        public int Id { get; set; }
        public int PieceId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public decimal Volume { get; set; }        // cm³
        public decimal SurfaceArea { get; set; }   // cm²
        public decimal BoundingBoxX { get; set; }  // mm
        public decimal BoundingBoxY { get; set; }  // mm
        public decimal BoundingBoxZ { get; set; }  // mm
        public decimal EstimatedWeight { get; set; } // g (volume * densité)
        public decimal EstimatedPrintTime { get; set; } // minutes
        public int TriangleCount { get; set; }
        public bool IsWatertight { get; set; }
        public bool HasErrors { get; set; }
        public string? Errors { get; set; }
        public DateTime AnalyzedAt { get; set; }

        // Navigation
        public Piece Piece { get; set; } = null!;
    }
}
