using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class FactureService : IFactureService
    {
        private readonly IFactureRepository _factureRepository;
        private readonly IAuditLogger _auditLogger;
        public FactureService(IFactureRepository factureRepository, IAuditLogger auditLogger)
        {
            _factureRepository = factureRepository;
            _auditLogger = auditLogger;
        }

        public async Task<Facture> CreateFromDevisAsync(Devis devis)
        {
            var lignes = devis.Lignes.Select(l => new FactureLigne
            {
                PieceId = l.PieceId,
                Description = l.Description,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire
            }).ToList();

            var facture = new Facture
            {
                DevisId = devis.Id,
                ClientId = devis.ClientId,
                DateEmission = DateTime.UtcNow,
                DateEcheance = DateTime.UtcNow.AddDays(30),
                TotalHT = devis.TotalHT,
                TVA = devis.TVA,
                TotalTTC = devis.TotalTTC,
                Statut = FactureStatus.Émise,
                Notes = $"Générée automatiquement depuis le devis {devis.NumeroDevis}",
                Lignes = lignes
            };

            var created = await _factureRepository.CreateAsync(facture);

            await _auditLogger.LogCreationAsync(EntityType.Facture, created.Id, created.NumeroFacture);

            return created;
        }

        public async Task<bool> ExistsForDevisAsync(int devisId)
        {
            return await _factureRepository.ExistsForDevisAsync(devisId);
        }

        public async Task<byte[]> GeneratePdfAsync(int id)
        {
            var facture = await GetByIdAsync(id);
            if (facture == null) return Array.Empty<byte>();

            // À implémenter avec une librairie PDF
            return Array.Empty<byte>();
        }

        public async Task<IEnumerable<Facture>> GetAllAsync()
        {
            return await _factureRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Facture>> GetByClientAsync(int clientId)
        {
            return await _factureRepository.GetByClientAsync(clientId);
        }

        public async Task<Facture?> GetByIdAsync(int id)
        {
            return await _factureRepository.GetByIdAsync(id);
        }

        public async Task<Facture?> UpdateStatutAsync(int id, FactureStatus statut)
        {
            var facture = await _factureRepository.GetByIdAsync(id);
            if (facture == null) return null;

            var oldStatut = facture.Statut;
            var updated = await _factureRepository.UpdateStatutAsync(id, statut);

            await _auditLogger.LogStatusChangeAsync(EntityType.Facture, id, oldStatut.ToString(), statut.ToString());

            return updated;
        }
    }
}
