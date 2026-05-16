using Backend.Models;

namespace Backend.Interface
{
    public interface IProjetRepository
    {
        Task<IEnumerable<Projet>> GetAllAsync();
        Task<Projet?> GetByIdAsync(int id);
        Task<Projet> CreateAsync(Projet projet);
        Task<Projet?> UpdateAsync(int id, Projet projet);
        Task<bool> DeleteAsync(int id);

        // ProjetPièces
        Task<ProjetPiece> AddPieceAsync(int projetId, int pieceId, int quantite);
        Task<bool> RemovePieceAsync(int projetId, int pieceId);
        Task<IEnumerable<ProjetPiece>> GetPiecesByProjetAsync(int projetId);
    }
}
