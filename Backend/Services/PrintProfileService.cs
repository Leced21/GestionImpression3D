using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class PrintProfileService : IPrintProfileService
    {
        private readonly IPrintProfileRepository _profileRepository;
        private readonly IPrinterRepository _printerRepository;
        private readonly IAuditLogger _auditLogger;
        public PrintProfileService(IPrintProfileRepository profileRepository, IPrinterRepository printerRepository, IAuditLogger auditLogger)
        {
            _profileRepository = profileRepository;
            _printerRepository = printerRepository;
            _auditLogger = auditLogger;
        }
        public async Task<PrintProfile> CreateAsync(CreatePrintProfileRequest request)
        {
            var printer = await _printerRepository.GetByIdAsync(request.PrinterId);
            if (printer == null)
                throw new InvalidOperationException("Imprimante non trouvée");

            var profile = new PrintProfile
            {
                Nom = request.Nom,
                Description = request.Description,
                PrinterId = request.PrinterId,
                Materiau = request.Materiau,
                NozzleTemp = request.NozzleTemp,
                BedTemp = request.BedTemp,
                LayerHeight = request.LayerHeight,
                Speed = request.Speed,
                Infill = request.Infill,
                InfillPattern = request.InfillPattern,
                Supports = request.Supports,
                SupportType = request.SupportType,
                MaterialMultiplier = request.MaterialMultiplier,
                IsDefault = request.IsDefault,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _profileRepository.CreateAsync(profile);

            await _auditLogger.LogCreationAsync(EntityType.PrintProfile, created.Id, created.Nom);

            return created;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            var result = await _profileRepository.DeleteAsync(id);

            if (result)
                await _auditLogger.LogDeletionAsync(EntityType.PrintProfile, id, profile.Nom);

            return result;
        }

        public async Task<PrintProfile?> DuplicateAsync(int id, string newName)
        {
            var original = await _profileRepository.GetByIdAsync(id);
            if (original == null) return null;

            var duplicate = new PrintProfile
            {
                Nom = newName,
                Description = $"Copie de {original.Nom}",
                PrinterId = original.PrinterId,
                Materiau = original.Materiau,
                NozzleTemp = original.NozzleTemp,
                BedTemp = original.BedTemp,
                LayerHeight = original.LayerHeight,
                Speed = original.Speed,
                Infill = original.Infill,
                InfillPattern = original.InfillPattern,
                Supports = original.Supports,
                SupportType = original.SupportType,
                MaterialMultiplier = original.MaterialMultiplier,
                IsDefault = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _profileRepository.CreateAsync(duplicate);

            await _auditLogger.LogCreationAsync(EntityType.PrintProfile, created.Id, created.Nom);

            return created;
        }

        public async Task<IEnumerable<PrintProfile>> GetAllAsync()
        {
            return await _profileRepository.GetAllAsync();
        }

        public async Task<PrintProfile?> GetByIdAsync(int id)
        {
            return await _profileRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<PrintProfile>> GetByMateriauAsync(string materiau)
        {
            return await _profileRepository.GetByMateriauAsync(materiau);
        }

        public async Task<IEnumerable<PrintProfile>> GetByPrinterAsync(int printerId)
        {
            return await _profileRepository.GetByPrinterAsync(printerId);
        }

        public async Task<PrintProfile?> GetDefaultForPrinterAsync(int printerId)
        {
            return await _profileRepository.GetDefaultForPrinterAsync(printerId);
        }

        public async Task<PrintProfileStatisticsDto> GetStatisticsAsync()
        {
            var profiles = await _profileRepository.GetAllAsync();
            var profilesList = profiles.ToList();

            var stats = new PrintProfileStatisticsDto
            {
                TotalProfiles = profilesList.Count,
                ActiveProfiles = profilesList.Count(p => p.IsActive),
                DefaultProfiles = profilesList.Count(p => p.IsDefault),
                CountByMateriau = profilesList
                    .GroupBy(p => p.Materiau)
                    .ToDictionary(g => g.Key, g => g.Count()),
                CountByPrinter = profilesList
                    .GroupBy(p => p.PrinterId)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return stats;
        }

        public async Task<PrintProfile?> SetDefaultAsync(int id)
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null) return null;

            await _profileRepository.SetDefaultAsync(profile.PrinterId, id);

            await _auditLogger.LogUpdateAsync(EntityType.PrintProfile, id, "IsDefault", "false", "true");

            return await _profileRepository.GetByIdAsync(id);
        }

        public async Task<PrintProfile?> UpdateAsync(int id, UpdatePrintProfileRequest request)
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null) return null;

            profile.Nom = request.Nom;
            profile.Description = request.Description;
            profile.Materiau = request.Materiau;
            profile.NozzleTemp = request.NozzleTemp;
            profile.BedTemp = request.BedTemp;
            profile.LayerHeight = request.LayerHeight;
            profile.Speed = request.Speed;
            profile.Infill = request.Infill;
            profile.InfillPattern = request.InfillPattern;
            profile.Supports = request.Supports;
            profile.SupportType = request.SupportType;
            profile.MaterialMultiplier = request.MaterialMultiplier;
            profile.IsActive = request.IsActive;

            var updated = await _profileRepository.UpdateAsync(profile);

            await _auditLogger.LogUpdateAsync(EntityType.PrintProfile, id, "Profile", "Modifié", updated.Nom);

            return updated;
        }
    }
}
