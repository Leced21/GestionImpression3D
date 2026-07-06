using Backend.Models;

namespace Backend.Interface
{
    public interface ISTLAnalyzerService
    {
        Task<STLMetadata> AnalyzeAsync(Stream stlStream, string fileName, int pieceId, string? materiau = null);
        Task<STLMetadata?> GetMetadataByPieceAsync(int pieceId);
        Task<byte[]> GeneratePreviewAsync(Stream stlStream);
        Task<SilhouetteData> ComputeSilhouetteAsync(Stream stlStream);
        bool IsSTLFile(Stream stream);
    }
}
