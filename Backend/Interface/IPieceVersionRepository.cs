using Backend.Models;

namespace Backend.Interface
{
    public interface IPieceVersionRepository
    {
        Task<PieceVersion?> GetByIdAsync(int id);
        Task<IEnumerable<PieceVersion>> GetByPieceIdAsync(int pieceId);
        Task<PieceVersion?> GetLatestVersionAsync(int pieceId);
        Task<PieceVersion> CreateAsync(PieceVersion version);
        Task<PieceVersion> UpdateAsync(PieceVersion version);
        Task<bool> DeleteAsync(int id);
        Task<int> GetNextVersionNumberAsync(int pieceId);
    }
}
