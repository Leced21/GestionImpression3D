using Backend.Data;
using Backend.Interface;
using Backend.Models;
using System.Numerics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
            // Génération d'une image PNG miniature (simplifiée)
            // Dans une version complète, utiliser une bibliothèque comme SixLabors.ImageSharp
            stlStream.Position = 0;

            // Retourner un placeholder pour l'instant
            return Array.Empty<byte>();
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
