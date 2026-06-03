using Backend.Models;

namespace Backend.Interface
{
    public interface IPieceVersionService
    {
        Task<IEnumerable<PieceVersion>> GetVersionsByPieceAsync(int pieceId);
        Task<PieceVersion?> GetVersionAsync(int versionId);
        Task<PieceVersion> CreateVersionAsync(int pieceId, Piece currentPiece, string changeLog, string createdBy, bool isPrototype = false);
        Task<PieceVersion?> PromoteToProductionAsync(int versionId);
        Task<bool> CompareVersionsAsync(int versionId1, int versionId2);
    }
}
