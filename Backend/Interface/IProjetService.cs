using Backend.Models;

namespace Backend.Interface
{
    public interface IProjetService
    {
        Task<IEnumerable<Projet>> GetAllAsync();
        Task<Projet?> GetByIdAsync(int id);
        Task<Projet> CreateAsync(Projet projet);
        Task<Projet?> UpdateAsync(int id, Projet projet);
        Task<bool> DeleteAsync(int id);

        // Gestion des pièces
        Task<ProjetPiece> AjouterPieceAsync(int projetId, int pieceId, int quantite);
        Task<bool> RetirerPieceAsync(int projetId, int pieceId);
        Task<IEnumerable<ProjetPiece>> GetPiecesAsync(int projetId);

        // Statistiques
        Task<object> GetStatsAsync(int id);
    }
}
