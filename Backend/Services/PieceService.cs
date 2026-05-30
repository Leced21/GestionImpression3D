using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class PieceService : IPieceService
    {
        private readonly IPieceRepository _pieceRepository;
        private readonly IAuditLogger _auditLogger;
        public PieceService(IPieceRepository pieceRepository, IAuditLogger auditLogger)
        {
            _pieceRepository = pieceRepository;
            _auditLogger = auditLogger;
        }
        public async Task<decimal> CalculerPrixRecommandéAsync(int id)
        {
            return await _pieceRepository.CalculerPrixRecommandéAsync(id);
        }

        public async Task<Piece> CreateAsync(Piece piece)
        {
            var created = await _pieceRepository.CreateAsync(piece);

            await _auditLogger.LogCreationAsync(
                EntityType.Piece,
                created.Id,
                created.Nom
            );

            return created;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _pieceRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var entityName = existing.Nom;

            await _pieceRepository.DeleteAsync(id);

            await _auditLogger.LogDeletionAsync(
                EntityType.Piece, id, entityName
            );

            return true;
        }

        public async Task<IEnumerable<Piece>> GetAllAsync()
        {
            return await _pieceRepository.GetAllAsync();
        }

        public async Task<Piece?> GetByIdAsync(int id)
        {
            return await _pieceRepository.GetByIdAsync(id);
        }

        public async Task<Piece> UpdateAsync(int id, Piece piece)
        {
            var existingPiece = await _pieceRepository.GetByIdAsync(id);
            if (existingPiece == null)
            {
                return null;
            }
            if (existingPiece.Nom != piece.Nom)
            {
                await _auditLogger.LogUpdateAsync(
                    EntityType.Piece, id, "Nom", existingPiece.Nom, piece.Nom
                );
            }

            if (existingPiece.PrixVente != piece.PrixVente)
            {
                await _auditLogger.LogUpdateAsync(
                    EntityType.Piece, id, "PrixVente",
                    existingPiece.PrixVente.ToString("F2"),
                    piece.PrixVente.ToString("F2")
                );
            }

            if (existingPiece.Description != piece.Description)
            {
                await _auditLogger.LogUpdateAsync(
                    EntityType.Piece, id, "Description",
                    existingPiece.Description ?? "",
                    piece.Description ?? ""
                );
            }

            piece.Id = id;
            piece.DateCreation = existingPiece.DateCreation;

            return await _pieceRepository.UpdateAsync(id, piece);
        }

        public async Task<Piece?> UpdateStatutAsync(int id, string nouveauStatut)
        {
            var existing = await _pieceRepository.GetByIdAsync(id);
            if (existing == null) return null;

            var oldStatut = existing.Statut;
            existing.Statut = nouveauStatut;
            await _pieceRepository.UpdateStatutAsync(id, nouveauStatut);
            await _auditLogger.LogStatusChangeAsync(EntityType.Piece, id, oldStatut, nouveauStatut);
            return existing;
        }
    }
}
