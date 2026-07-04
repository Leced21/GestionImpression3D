using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class OrdreFabricationService : IOrdreFabricationService
    {
        private readonly IOrdreFabricationRepository _ordreRepository;
        private readonly IProjetRepository _projetRepository;
        private readonly IPieceRepository _pieceRepository;
        private readonly IAuditLogger _auditLogger;
        public OrdreFabricationService(IOrdreFabricationRepository ordreRepository, IProjetRepository projetRepository, IPieceRepository pieceRepository, IAuditLogger auditLogger)
        {
            _ordreRepository = ordreRepository;
            _projetRepository = projetRepository;
            _pieceRepository = pieceRepository;
            _auditLogger = auditLogger;
        }
        public async Task<OrdreFabrication?> CompleteProductionAsync(int id)
        {
            var ordre = await _ordreRepository.GetByIdAsync(id);
            if (ordre == null) return null;

            ordre.QuantiteProduite = ordre.Quantite;

            return await UpdateStatutAsync(id, OrdreStatut.Termine);
        }

        public async Task<OrdreFabrication> CreateAsync(CreateOrdreRequest request)
        {
            var projet = await _projetRepository.GetByIdAsync(request.ProjetId);
            if (projet == null)
                throw new InvalidOperationException("Projet non trouvé");

            var piece = await _pieceRepository.GetByIdAsync(request.PieceId);
            if (piece == null)
                throw new InvalidOperationException("Pièce non trouvée");

            var nextNumber = await _ordreRepository.GetNextReferenceNumberAsync();
            var reference = $"OF-{nextNumber:D4}";

            var ordre = new OrdreFabrication
            {
                Reference = reference,
                ProjetId = request.ProjetId,
                PieceId = request.PieceId,
                DevisId = request.DevisId,
                Quantite = request.Quantite,
                QuantiteProduite = 0,
                Statut = OrdreStatut.EnAttente,
                Priorite = request.Priorite,
                DateEcheance = request.DateEcheance,
                DateCreation = DateTime.UtcNow,
                Notes = request.Notes
            };

            var created = await _ordreRepository.CreateAsync(ordre);

            await _auditLogger.LogCreationAsync(EntityType.OrdreFabrication, created.Id, created.Reference);

            return created;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ordre = await _ordreRepository.GetByIdAsync(id);
            if (ordre == null) return false;

            var result = await _ordreRepository.DeleteAsync(id);

            if (result)
                await _auditLogger.LogDeletionAsync(EntityType.OrdreFabrication, id, ordre.Reference);

            return result;
        }

        public async Task<IEnumerable<OrdreFabrication>> GetAllAsync()
        {
            return await _ordreRepository.GetAllAsync();
        }

        public async Task<OrdreFabrication?> GetByIdAsync(int id)
        {
            return await _ordreRepository.GetByIdAsync(id);
        }

        public async Task<OrdreStatisticsDto> GetStatisticsAsync()
        {
            var ordres = await _ordreRepository.GetAllAsync();
            var ordresList = ordres.ToList();

            var enRetard = ordresList.Count(o => o.DateEcheance.HasValue &&
                                                  o.DateEcheance < DateTime.UtcNow &&
                                                  o.Statut != OrdreStatut.Termine &&
                                                  o.Statut != OrdreStatut.Annule);

            return new OrdreStatisticsDto
            {
                TotalOrdres = ordresList.Count,
                EnAttente = ordresList.Count(o => o.Statut == OrdreStatut.EnAttente),
                EnCours = ordresList.Count(o => o.Statut == OrdreStatut.EnCours),
                Termines = ordresList.Count(o => o.Statut == OrdreStatut.Termine),
                Annules = ordresList.Count(o => o.Statut == OrdreStatut.Annule),
                EnRetard = enRetard,
                QuantiteTotale = ordresList.Sum(o => o.Quantite),
                QuantiteProduite = ordresList.Sum(o => o.QuantiteProduite),
                TauxAvancement = ordresList.Any() ? (decimal)ordresList.Sum(o => o.QuantiteProduite) / ordresList.Sum(o => o.Quantite) * 100 : 0
            };
        }

        public async Task<OrdreFabrication?> StartProductionAsync(int id)
        {
            return await UpdateStatutAsync(id, OrdreStatut.EnCours);
        }

        public async Task<bool> ExistsForDevisAsync(int devisId)
        {
            return await _ordreRepository.ExistsForDevisAsync(devisId);
        }

        public async Task<OrdreFabrication?> UpdateAsync(int id, UpdateOrdreRequest request)
        {
            var ordre = await _ordreRepository.GetByIdAsync(id);
            if (ordre == null) return null;

            ordre.Quantite = request.Quantite;
            ordre.Priorite = request.Priorite;
            ordre.DateEcheance = request.DateEcheance;
            ordre.Notes = request.Notes;

            var updated = await _ordreRepository.UpdateAsync(ordre);

            await _auditLogger.LogUpdateAsync(EntityType.OrdreFabrication, id, "Ordre", "", "Modifié");

            return updated;
        }

        public async Task<OrdreFabrication?> UpdateStatutAsync(int id, OrdreStatut statut)
        {
            var ordre = await _ordreRepository.GetByIdAsync(id);
            if (ordre == null) return null;

            var oldStatut = ordre.Statut;
            ordre.Statut = statut;

            if (statut == OrdreStatut.EnCours && oldStatut != OrdreStatut.EnCours)
                ordre.DateDebut = DateTime.UtcNow;

            if (statut == OrdreStatut.Termine)
                ordre.DateFin = DateTime.UtcNow;

            var updated = await _ordreRepository.UpdateAsync(ordre);

            await _auditLogger.LogStatusChangeAsync(EntityType.OrdreFabrication, id, oldStatut.ToString(), statut.ToString());

            return updated;
        }
    }
}
