using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class PieceVersionService : IPieceVersionService
    {
        private readonly IPieceRepository _pieceRepository;
        private readonly IPieceVersionRepository _versionRepository;
        private readonly IAuditLogger _auditLogger;
        public PieceVersionService(IPieceRepository pieceRepository, IPieceVersionRepository versionRepository, IAuditLogger auditLogger)
        {
            _pieceRepository = pieceRepository;
            _versionRepository = versionRepository;
            _auditLogger = auditLogger;
        }
        public async Task<bool> CompareVersionsAsync(int versionId1, int versionId2)
        {
            var v1 = await _versionRepository.GetByIdAsync(versionId1);
            var v2 = await _versionRepository.GetByIdAsync(versionId2);

            if (v1 == null || v2 == null) return false;

            // Logique de comparaison (retourne true si différentes)
            return v1.Nom != v2.Nom ||
                   v1.Description != v2.Description ||
                   v1.CoutMatiere != v2.CoutMatiere ||
                   v1.CoutMachine != v2.CoutMachine ||
                   v1.CoutMainOeuvre != v2.CoutMainOeuvre ||
                   v1.PrixVente != v2.PrixVente;
        }

        public async Task<PieceVersion> CreateVersionAsync(int pieceId, Piece currentPiece, string changeLog, string createdBy, bool isPrototype = false)
        {
            var nextVersion = await _versionRepository.GetNextVersionNumberAsync(pieceId);

            var version = new PieceVersion
            {
                PieceId = pieceId,
                VersionNumber = nextVersion,
                Nom = currentPiece.Nom,
                Description = currentPiece.Description,
                CoutMatiere = currentPiece.CoutMatiere,
                CoutMachine = currentPiece.CoutMachine,
                CoutMainOeuvre = currentPiece.CoutMainOeuvre,
                PrixVente = currentPiece.PrixVente,
                StlFileName = currentPiece.StlFileName,
                ChangeLog = changeLog,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsPrototype = isPrototype
            };

            var created = await _versionRepository.CreateAsync(version);

            await _auditLogger.LogCreationAsync(EntityType.Piece, created.Id, $"Version {nextVersion} de {currentPiece.Nom}");

            return created;
        }

        public async Task<PieceVersion?> GetVersionAsync(int versionId)
        {
            return await _versionRepository.GetByIdAsync(versionId);
        }

        public async Task<IEnumerable<PieceVersion>> GetVersionsByPieceAsync(int pieceId)
        {
            return await _versionRepository.GetByPieceIdAsync(pieceId);
        }

        public async Task<PieceVersion?> PromoteToProductionAsync(int versionId)
        {
            var version = await _versionRepository.GetByIdAsync(versionId);
            if (version == null) return null;

            version.IsPrototype = false;
            var updated = await _versionRepository.UpdateAsync(version);

            await _auditLogger.LogUpdateAsync(EntityType.Piece, version.PieceId, "VersionStatus", "Prototype", "Production");

            return updated;
        }
    }
}
