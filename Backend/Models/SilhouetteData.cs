namespace Backend.Models
{
    // Segment de silhouette (arête de contour ou arête vive) projeté en 2D, en mm réels.
    public class SilhouetteEdge
    {
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
    }

    // Contours des 3 vues orthographiques standard d'une pièce, calculés à partir du
    // maillage STL réel (et non d'une simple boîte englobante).
    public class SilhouetteData
    {
        public List<SilhouetteEdge> Front { get; set; } = new(); // plan (X, Z)
        public List<SilhouetteEdge> Top { get; set; } = new();   // plan (X, Y)
        public List<SilhouetteEdge> Side { get; set; } = new();  // plan (Y, Z)
    }
}
