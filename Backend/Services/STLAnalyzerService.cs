using Backend.Data;
using Backend.Interface;
using Backend.Models;
using System.Numerics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SkiaSharp;

namespace Backend.Services
{
    public class STLAnalyzerService : ISTLAnalyzerService
    {
        private readonly IServiceProvider _serviceProvider;
        private const decimal DensityPLA = 1.24m; // g/cm³
        private const decimal DensityPETG = 1.27m;
        private const decimal DensityABS = 1.04m;
        private const decimal DensityResine = 1.10m;
        private const decimal PrintSpeed = 60; // mm/s
        public STLAnalyzerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task<STLMetadata> AnalyzeAsync(Stream stlStream, string fileName, int pieceId, string? materiau = null)
        {
            // Réinitialiser la position du stream
            stlStream.Position = 0;

            var triangles = new List<Triangle>();

            // Détecter si c'est un STL ASCII ou binaire
            var isAscii = IsAsciiSTL(stlStream);
            stlStream.Position = 0;

            if (isAscii)
            {
                triangles = await ParseAsciiSTLAsync(stlStream);
            }
            else
            {
                triangles = await ParseBinarySTLAsync(stlStream);
            }

            // Calculer les métriques. Les sommets STL sont en mm, donc CalculateVolume/
            // CalculateSurfaceArea renvoient des mm³/mm² bruts : on convertit ici en cm³/cm²
            // (unités attendues par STLMetadata et par le calcul du poids/temps d'impression),
            // sans quoi le poids estimé se retrouve 1000x trop grand.
            var volumeMm3 = CalculateVolume(triangles);
            var surfaceAreaMm2 = CalculateSurfaceArea(triangles);
            var volume = volumeMm3 / 1000m;
            var surfaceArea = surfaceAreaMm2 / 100m;
            var bounds = CalculateBoundingBox(triangles);
            var isWatertight = CheckWatertight(triangles);

            // Estimer le poids selon la densité du matériau réellement utilisé
            var estimatedWeight = volume * GetDensity(materiau);

            // Estimer le temps d'impression
            var estimatedPrintTime = EstimatePrintTime(volume, surfaceArea);

            var metadata = new STLMetadata
            {
                PieceId = pieceId,
                FileName = fileName,
                FileSize = stlStream.Length,
                Volume = Math.Round(volume, 2),
                SurfaceArea = Math.Round(surfaceArea, 2),
                BoundingBoxX = Math.Round((decimal)bounds.X, 2),
                BoundingBoxY = Math.Round((decimal)bounds.Y, 2),
                BoundingBoxZ = Math.Round((decimal)bounds.Z, 2),
                EstimatedWeight = Math.Round(estimatedWeight, 2),
                EstimatedPrintTime = Math.Round(estimatedPrintTime, 0),
                TriangleCount = triangles.Count,
                IsWatertight = isWatertight,
                HasErrors = triangles.Count == 0,
                AnalyzedAt = DateTime.UtcNow
            };

            return metadata;
        }

        private static decimal GetDensity(string? materiau) => materiau?.Trim().ToUpperInvariant() switch
        {
            "PETG" => DensityPETG,
            "ABS" => DensityABS,
            "RÉSINE" or "RESINE" => DensityResine,
            _ => DensityPLA
        };

        public async Task<byte[]> GeneratePreviewAsync(Stream stlStream)
        {
            stlStream.Position = 0;

            var isAscii = IsAsciiSTL(stlStream);
            stlStream.Position = 0;

            var triangles = isAscii
                ? await ParseAsciiSTLAsync(stlStream)
                : await ParseBinarySTLAsync(stlStream);

            if (triangles.Count == 0)
                return Array.Empty<byte>();

            return RenderTrianglesToPng(triangles);
        }

        public async Task<SilhouetteData> ComputeSilhouetteAsync(Stream stlStream)
        {
            stlStream.Position = 0;

            var isAscii = IsAsciiSTL(stlStream);
            stlStream.Position = 0;

            var triangles = isAscii
                ? await ParseAsciiSTLAsync(stlStream)
                : await ParseBinarySTLAsync(stlStream);

            if (triangles.Count == 0)
                return new SilhouetteData();

            return new SilhouetteData
            {
                Front = ComputeSilhouetteEdges(triangles, new Vector3(0, 1, 0), v => (v.X, v.Z)),
                Top = ComputeSilhouetteEdges(triangles, new Vector3(0, 0, 1), v => (v.X, v.Y)),
                Side = ComputeSilhouetteEdges(triangles, new Vector3(1, 0, 0), v => (v.Y, v.Z))
            };
        }

        // Détection de silhouette façon logiciel de CAO : une arête 3D est gardée si elle est
        // au bord du maillage (un seul triangle adjacent) ou si les deux triangles qui la
        // partagent sont respectivement visible/caché par rapport à la direction d'observation
        // (leurs normales ont un produit scalaire de signe opposé avec viewDir). On regroupe les
        // arêtes par les mêmes clés (hashcode des sommets) que CheckWatertight, ce qui suppose un
        // STL correctement soudé (sommets partagés bit-identiques), comme pour cette méthode.
        private static List<SilhouetteEdge> ComputeSilhouetteEdges(List<Triangle> triangles, Vector3 viewDir, Func<Vector3, (float U, float V)> project)
        {
            var edgeGroups = new Dictionary<(int, int), List<int>>();
            var vertexLookup = new Dictionary<int, Vector3>();
            var normals = new Vector3[triangles.Count];

            void AddEdge(int triangleIndex, Vector3 a, Vector3 b)
            {
                var ha = a.GetHashCode();
                var hb = b.GetHashCode();
                vertexLookup[ha] = a;
                vertexLookup[hb] = b;

                var key = ha < hb ? (ha, hb) : (hb, ha);
                if (!edgeGroups.TryGetValue(key, out var list))
                {
                    list = new List<int>();
                    edgeGroups[key] = list;
                }
                list.Add(triangleIndex);
            }

            for (int i = 0; i < triangles.Count; i++)
            {
                var t = triangles[i];
                var normal = Vector3.Cross(t.V2 - t.V1, t.V3 - t.V1);
                normals[i] = normal.LengthSquared() > 1e-10f ? Vector3.Normalize(normal) : Vector3.Zero;

                AddEdge(i, t.V1, t.V2);
                AddEdge(i, t.V2, t.V3);
                AddEdge(i, t.V3, t.V1);
            }

            // Seuil d'angle dièdre au-delà duquel une arête interne (pas sur le contour de
            // silhouette) est jugée assez marquée pour être tracée comme arête vive (nervure,
            // bord de trou, arête d'un chanfrein...), comme le ferait un logiciel de CAO en
            // mode "silhouette + arêtes vives" plutôt qu'un simple contour extérieur.
            const float sharpEdgeThresholdRadians = 25f * MathF.PI / 180f;

            var edges = new List<SilhouetteEdge>();

            foreach (var (key, triIndices) in edgeGroups)
            {
                bool isSilhouette;
                bool isHidden = false;

                if (triIndices.Count == 1)
                {
                    // Arête de bord du maillage : toujours sur le contour visible de la pièce.
                    isSilhouette = true;
                }
                else if (triIndices.Count == 2)
                {
                    var n1 = normals[triIndices[0]];
                    var n2 = normals[triIndices[1]];
                    var facing1 = Vector3.Dot(n1, viewDir) >= 0;
                    var facing2 = Vector3.Dot(n2, viewDir) >= 0;

                    if (facing1 != facing2)
                    {
                        // Transition visible/caché : c'est exactement le contour de
                        // silhouette, toujours visible par définition.
                        isSilhouette = true;
                    }
                    else
                    {
                        var angle = MathF.Acos(Math.Clamp(Vector3.Dot(n1, n2), -1f, 1f));
                        isSilhouette = angle > sharpEdgeThresholdRadians;
                        // facing1 == facing2 ici : les deux faces sont soit du côté visible,
                        // soit toutes les deux cachées derrière la pièce vue de cet axe.
                        isHidden = isSilhouette && !facing1;
                    }
                }
                else
                {
                    // Arête partagée par 3+ triangles (maillage non-manifold/dégénéré) :
                    // ignorée plutôt que de risquer un rendu incohérent.
                    isSilhouette = false;
                }

                if (!isSilhouette) continue;
                if (!vertexLookup.TryGetValue(key.Item1, out var a) || !vertexLookup.TryGetValue(key.Item2, out var b))
                    continue;

                var (u1, v1) = project(a);
                var (u2, v2) = project(b);
                edges.Add(new SilhouetteEdge { X1 = u1, Y1 = v1, X2 = u2, Y2 = v2, IsHidden = isHidden });
            }

            return edges;
        }

        // Rendu 3D simplifié (vue isométrique, tri peintre + ombrage lambertien) directement
        // à partir des triangles STL : pas de bibliothèque de rendu 3D disponible/nécessaire,
        // SkiaSharp (déjà utilisé en interne par QuestPDF) suffit pour rasteriser les faces.
        private static byte[] RenderTrianglesToPng(List<Triangle> triangles)
        {
            const int size = 800;
            const int margin = 60;

            var rotY = -MathF.PI / 4f;
            var rotX = MathF.Atan(1f / MathF.Sqrt(2f));

            Vector3 Rotate(Vector3 v)
            {
                var x1 = v.X * MathF.Cos(rotY) + v.Z * MathF.Sin(rotY);
                var z1 = -v.X * MathF.Sin(rotY) + v.Z * MathF.Cos(rotY);
                var y1 = v.Y;

                var y2 = y1 * MathF.Cos(rotX) - z1 * MathF.Sin(rotX);
                var z2 = y1 * MathF.Sin(rotX) + z1 * MathF.Cos(rotX);

                return new Vector3(x1, y2, z2);
            }

            var rotated = triangles
                .Select(t => (V1: Rotate(t.V1), V2: Rotate(t.V2), V3: Rotate(t.V3)))
                .ToList();

            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;

            foreach (var t in rotated)
            {
                foreach (var v in new[] { t.V1, t.V2, t.V3 })
                {
                    minX = Math.Min(minX, v.X);
                    maxX = Math.Max(maxX, v.X);
                    minY = Math.Min(minY, v.Y);
                    maxY = Math.Max(maxY, v.Y);
                }
            }

            var spanX = Math.Max(maxX - minX, 0.001f);
            var spanY = Math.Max(maxY - minY, 0.001f);
            var scale = (size - 2 * margin) / Math.Max(spanX, spanY);
            var centerX = (minX + maxX) / 2f;
            var centerY = (minY + maxY) / 2f;

            SKPoint Project(Vector3 v) => new SKPoint(
                size / 2f + (v.X - centerX) * scale,
                size / 2f - (v.Y - centerY) * scale
            );

            var ordered = rotated
                .OrderBy(t => (t.V1.Z + t.V2.Z + t.V3.Z) / 3f)
                .ToList();

            var lightDir = Vector3.Normalize(new Vector3(0.4f, 0.6f, 1f));

            using var bitmap = new SKBitmap(size, size);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(new SKColor(0xF5, 0xF5, 0xF5));

                foreach (var t in ordered)
                {
                    var normal = Vector3.Cross(t.V2 - t.V1, t.V3 - t.V1);
                    if (normal.LengthSquared() < 1e-8f) continue;
                    normal = Vector3.Normalize(normal);

                    var intensity = Math.Clamp(Vector3.Dot(normal, lightDir), 0.25f, 1f);
                    var baseShade = (byte)Math.Min(255f, intensity * 255f);

                    var color = new SKColor(
                        (byte)Math.Min(255f, 0x0E * intensity + baseShade * 0.15f),
                        (byte)Math.Min(255f, 0x28 * intensity + baseShade * 0.15f),
                        (byte)Math.Min(255f, 0x41 + baseShade * 0.5f)
                    );

                    using var path = new SKPath();
                    path.MoveTo(Project(t.V1));
                    path.LineTo(Project(t.V2));
                    path.LineTo(Project(t.V3));
                    path.Close();

                    using var paint = new SKPaint
                    {
                        Color = color,
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill
                    };
                    canvas.DrawPath(path, paint);
                }

                // Repère XYZ dans le coin, à la même orientation que la pièce : aide à situer
                // les axes, comme le petit repère toujours présent dans les logiciels de CAO.
                DrawAxisTriad(canvas, Rotate, new SKPoint(55, size - 55));
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static void DrawAxisTriad(SKCanvas canvas, Func<Vector3, Vector3> rotate, SKPoint origin)
        {
            const float axisLength = 35f;
            var axes = new (Vector3 Dir, string Label, SKColor Color)[]
            {
                (new Vector3(1, 0, 0), "X", new SKColor(0xC0, 0x30, 0x30)),
                (new Vector3(0, 1, 0), "Y", new SKColor(0x20, 0x90, 0x40)),
                (new Vector3(0, 0, 1), "Z", new SKColor(0x20, 0x40, 0x90))
            };

            foreach (var (dir, label, color) in axes)
            {
                var rotated = rotate(dir);
                var end = new SKPoint(origin.X + rotated.X * axisLength, origin.Y - rotated.Y * axisLength);

                using var linePaint = new SKPaint
                {
                    Color = color,
                    StrokeWidth = 2f,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };
                canvas.DrawLine(origin, end, linePaint);

                using var textPaint = new SKPaint
                {
                    Color = color,
                    IsAntialias = true,
                    TextSize = 12f,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText(label, end.X + (end.X - origin.X) * 0.2f, end.Y + (end.Y - origin.Y) * 0.2f, textPaint);
            }
        }

        public async Task<STLMetadata?> GetMetadataByPieceAsync(int pieceId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await context.STLMetadata
                .FirstOrDefaultAsync(m => m.PieceId == pieceId);
        }

        public bool IsSTLFile(Stream stream)
        {
            stream.Position = 0;
            var header = new byte[84];
            var bytesRead = stream.Read(header, 0, header.Length);
            stream.Position = 0;

            if (bytesRead < 84)
                return false;

            // Vérifier l'en-tête STL binaire (80 bytes + 4 bytes triangle count)
            var possibleTriangleCount = BitConverter.ToInt32(header, 80);

            // Vérifier si c'est un STL ASCII
            var firstChars = Encoding.ASCII.GetString(header, 0, Math.Min(80, header.Length));
            return firstChars.TrimStart().StartsWith("solid") || (possibleTriangleCount > 0 && possibleTriangleCount < 1000000);
        }

        private bool IsAsciiSTL(Stream stream)
        {
            var buffer = new byte[200];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            stream.Position = 0;

            var text = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            return text.TrimStart().StartsWith("solid", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<List<Triangle>> ParseAsciiSTLAsync(Stream stream)
        {
            var triangles = new List<Triangle>();
            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);

            string? line;
            Vector3? normal = null;
            var vertices = new List<Vector3>();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) continue;

                if (parts[0].Equals("facet", StringComparison.OrdinalIgnoreCase) && parts.Length >= 4)
                {
                    normal = new Vector3(
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture),
                        float.Parse(parts[4], CultureInfo.InvariantCulture)
                    );
                    vertices.Clear();
                }
                else if (parts[0].Equals("vertex", StringComparison.OrdinalIgnoreCase) && parts.Length >= 4)
                {
                    vertices.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)
                    ));

                    if (vertices.Count == 3)
                    {
                        triangles.Add(new Triangle(vertices[0], vertices[1], vertices[2], normal ?? Vector3.Zero));
                        vertices.Clear();
                    }
                }
            }

            return triangles;
        }

        private async Task<List<Triangle>> ParseBinarySTLAsync(Stream stream)
        {
            var triangles = new List<Triangle>();
            using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

            // Skip header (80 bytes)
            reader.ReadBytes(80);

            // Read triangle count (4 bytes)
            var triangleCount = reader.ReadInt32();

            for (int i = 0; i < triangleCount; i++)
            {
                // Normal vector (12 bytes)
                var nx = reader.ReadSingle();
                var ny = reader.ReadSingle();
                var nz = reader.ReadSingle();
                var normal = new Vector3(nx, ny, nz);

                // Vertices (3 * 12 bytes)
                var v1x = reader.ReadSingle();
                var v1y = reader.ReadSingle();
                var v1z = reader.ReadSingle();
                var v1 = new Vector3(v1x, v1y, v1z);

                var v2x = reader.ReadSingle();
                var v2y = reader.ReadSingle();
                var v2z = reader.ReadSingle();
                var v2 = new Vector3(v2x, v2y, v2z);

                var v3x = reader.ReadSingle();
                var v3y = reader.ReadSingle();
                var v3z = reader.ReadSingle();
                var v3 = new Vector3(v3x, v3y, v3z);

                // Attribute byte count (2 bytes)
                reader.ReadUInt16();

                triangles.Add(new Triangle(v1, v2, v3, normal));
            }

            return triangles;
        }

        private decimal CalculateVolume(List<Triangle> triangles)
        {
            decimal volume = 0;

            foreach (var triangle in triangles)
            {
                // Volume du tétraèdre formé par le triangle et l'origine
                var v321 = Vector3.Dot(triangle.V3, Vector3.Cross(triangle.V2, triangle.V1));
                volume += (decimal)v321;
            }

            return Math.Abs(volume) / 6;
        }

        private decimal CalculateSurfaceArea(List<Triangle> triangles)
        {
            decimal area = 0;

            foreach (var triangle in triangles)
            {
                var ab = triangle.V2 - triangle.V1;
                var ac = triangle.V3 - triangle.V1;
                var cross = Vector3.Cross(ab, ac);
                area += (decimal)cross.Length() / 2;
            }

            return area;
        }

        private Vector3 CalculateBoundingBox(List<Triangle> triangles)
        {
            if (triangles.Count == 0) return Vector3.Zero;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var triangle in triangles)
            {
                min = Vector3.Min(min, triangle.V1);
                min = Vector3.Min(min, triangle.V2);
                min = Vector3.Min(min, triangle.V3);

                max = Vector3.Max(max, triangle.V1);
                max = Vector3.Max(max, triangle.V2);
                max = Vector3.Max(max, triangle.V3);
            }

            return new Vector3(max.X - min.X, max.Y - min.Y, max.Z - min.Z);
        }

        private bool CheckWatertight(List<Triangle> triangles)
        {
            // Vérification simplifiée : chaque arête doit apparaître exactement 2 fois
            var edgeCounts = new Dictionary<(int, int, int), int>();

            foreach (var triangle in triangles)
            {
                var edges = new[] {
                (triangle.V1, triangle.V2),
                (triangle.V2, triangle.V3),
                (triangle.V3, triangle.V1)
            };

                foreach (var edge in edges)
                {
                    var key = (edge.Item1.GetHashCode(), edge.Item2.GetHashCode(), 0);
                    if (!edgeCounts.ContainsKey(key))
                        edgeCounts[key] = 0;
                    edgeCounts[key]++;
                }
            }

            return edgeCounts.Values.All(count => count == 2);
        }

        private decimal EstimatePrintTime(decimal volume, decimal surfaceArea)
        {
            // Estimation basée sur le volume et la surface
            // Volume en cm³, surface en cm²
            var timeForVolume = volume * 20; // 20 minutes par cm³
            var timeForSurface = surfaceArea * 0.5m; // 0.5 minutes par cm²
            var totalMinutes = timeForVolume + timeForSurface;

            return totalMinutes;
        }

        private struct Triangle
        {
            public Vector3 V1 { get; }
            public Vector3 V2 { get; }
            public Vector3 V3 { get; }
            public Vector3 Normal { get; }

            public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                Normal = normal;
            }
        }
    }
}
