namespace Backend.Models
{
    // Segment de silhouette (contour, arête vive ou ligne cachée) projeté en 2D, en mm réels.
    public class SilhouetteEdge
    {
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }

        // Vrai si l'arête est du côté de la pièce opposé à l'observateur (approximation par
        // orientation des faces adjacentes) : tracée en pointillés, comme une ligne cachée
        // sur un plan CAO classique, plutôt qu'omise.
        public bool IsHidden { get; set; }
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
