using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Moq;


namespace TestProject
{
    public class PrintProfileServiceTests
    {
        private readonly IPrintProfileService _profileService;
        private readonly Mock<IPrintProfileRepository> _profileRepositoryMock;
        private readonly Mock<IPrinterRepository> _printerRepositoryMock;
        private readonly Mock<IAuditLogger> _auditLoggerMock;

        public PrintProfileServiceTests()
        {
            _profileRepositoryMock = new Mock<IPrintProfileRepository>();
            _printerRepositoryMock = new Mock<IPrinterRepository>();
            _auditLoggerMock = new Mock<IAuditLogger>();

            _profileService = new PrintProfileService(
                _profileRepositoryMock.Object,
                _printerRepositoryMock.Object,
                _auditLoggerMock.Object
            );
        }
        private Printer CreatePrinter(int id = 1, string nom = "Prusa MK4")
        {
            return Printer.Create(
                nom: nom,
                reference: $"PRINTER-{id}",
                model: "MK4",
                brand: "Prusa",
                type: PrinterType.FDM,
                ipAddress: "192.168.1.100",
                maxSizeX: 250,
                maxSizeY: 210,
                maxSizeZ: 210
            );
        }
        private PrintProfile CreateProfile(int id = 1, string nom = "PLA Standard", int printerId = 1)
        {
            // Utiliser le constructeur public ou un factory method si disponible
            var profile = new PrintProfile
            {
                Id = id,
                Nom = nom,
                PrinterId = printerId,
                NozzleTemp = 210,
                BedTemp = 60,
                LayerHeight = 0.20m,
                Speed = 60,
                Infill = 20,
                Supports = false,
                IsDefault = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            return profile;
        }

        [Fact]
        public async Task CreateProfile_WithValidData_ReturnsProfile()
        {
            // Arrange
            var request = new CreatePrintProfileRequest
            {
                Nom = "PLA Standard",
                PrinterId = 1,
                NozzleTemp = 210,
                BedTemp = 60,
                LayerHeight = 0.20m,
                Speed = 60,
                Infill = 20,
                Supports = false,
                IsDefault = false
            };

            var printer = CreatePrinter(1);
            var profile = CreateProfile(1, "PLA Standard", 1);
        
            _printerRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(printer);
            _profileRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<PrintProfile>())).ReturnsAsync(profile);

            // Act
            var result = await _profileService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PLA Standard", result.Nom);
            Assert.Equal(210, result.NozzleTemp);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task CreateProfile_WithInvalidPrinter_ThrowsException()
        {
            // Arrange
            var request = new CreatePrintProfileRequest
            {
                Nom = "PLA Standard",
                PrinterId = 999,
                NozzleTemp = 210
            };

            _printerRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Printer?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _profileService.CreateAsync(request)
            );
        }

        [Fact]
        public async Task GetProfileById_ExistingProfile_ReturnsProfile()
        {
            // Arrange
            var profile = new PrintProfile
            {
                Id = 1,
                Nom = "PLA Standard",
                NozzleTemp = 210
            };
            _profileRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(profile);

            // Act
            var result = await _profileService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("PLA Standard", result.Nom);
        }

        [Fact]
        public async Task GetProfileById_NonExistingProfile_ReturnsNull()
        {
            // Arrange
            _profileRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((PrintProfile?)null);

            // Act
            var result = await _profileService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllProfiles_ReturnsListOfProfiles()
        {
            // Arrange
            var profiles = new List<PrintProfile>
            {
                new PrintProfile { Id = 1, Nom = "PLA Standard" },
                new PrintProfile { Id = 2, Nom = "PLA Flexible" }
            };
            _profileRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(profiles);

            // Act
            var result = await _profileService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetProfilesByPrinter_ReturnsProfiles()
        {
            // Arrange
            var profiles = new List<PrintProfile>
            {
                new PrintProfile { Id = 1, Nom = "PLA Standard", PrinterId = 1 },
                new PrintProfile { Id = 2, Nom = "PETG", PrinterId = 1 }
            };
            _profileRepositoryMock.Setup(x => x.GetByPrinterAsync(1)).ReturnsAsync(profiles);

            // Act
            var result = await _profileService.GetByPrinterAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(1, p.PrinterId));
        }

        [Fact]
        public async Task SetDefaultProfile_UpdatesDefaultStatus()
        {
            // Arrange
            var profile = new PrintProfile
            {
                Id = 1,
                Nom = "PLA Standard",
                PrinterId = 1,
                IsDefault = false
            };

            _profileRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(profile);
            _profileRepositoryMock.Setup(x => x.SetDefaultAsync(1, 1)).ReturnsAsync(true);
            _profileRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new PrintProfile { Id = 1, Nom = "PLA Standard", PrinterId = 1, IsDefault = true });

            // Act
            var result = await _profileService.SetDefaultAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDefault);
        }

        [Fact]
        public async Task DuplicateProfile_CreatesCopy()
        {
            // Arrange
            var original = new PrintProfile
            {
                Id = 1,
                Nom = "PLA Standard",
                PrinterId = 1,
                NozzleTemp = 210,
                BedTemp = 60,
                LayerHeight = 0.20m,
                Supports = false,
                IsDefault = false
            };

            var duplicate = new PrintProfile
            {
                Id = 2,
                Nom = "Copy of PLA Standard",
                PrinterId = 1,
                NozzleTemp = 210,
                BedTemp = 60,
                LayerHeight = 0.20m,
                Supports = false,
                IsDefault = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _profileRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(original);
            _profileRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<PrintProfile>())).ReturnsAsync(duplicate);

            // Act
            var result = await _profileService.DuplicateAsync(1, "Copy of PLA Standard");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Copy of PLA Standard", result.Nom);
            Assert.Equal(1, result.PrinterId);
            Assert.Equal(210, result.NozzleTemp);
        }

        [Fact]
        public async Task UpdateProfile_WithValidData_ReturnsUpdatedProfile()
        {
            // Arrange
            var existingProfile = new PrintProfile
            {
                Id = 1,
                Nom = "Old Name",
                NozzleTemp = 200,
                IsActive = true
            };
            var updateRequest = new UpdatePrintProfileRequest
            {
                Nom = "New Name",
                NozzleTemp = 220,
                IsActive = true
            };

            _profileRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingProfile);
            _profileRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<PrintProfile>()))
                .ReturnsAsync((PrintProfile p) => { p.Nom = "New Name"; p.NozzleTemp = 220; return p; });

            // Act
            var result = await _profileService.UpdateAsync(1, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Nom);
            Assert.Equal(220, result.NozzleTemp);
        }

        [Fact]
        public async Task DeleteProfile_ExistingProfile_ReturnsTrue()
        {
            // Arrange
            var profile = new PrintProfile { Id = 1, Nom = "Test Profile" };
            _profileRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(profile);
            _profileRepositoryMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _profileService.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProfile_NonExistingProfile_ReturnsFalse()
        {
            // Arrange
            _profileRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((PrintProfile?)null);

            // Act
            var result = await _profileService.DeleteAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetStatistics_ReturnsCorrectStats()
        {
            // Arrange
            var profiles = new List<PrintProfile>
            {
                new PrintProfile { Id = 1, Nom = "PLA Standard", IsActive = true, IsDefault = true, Materiau = "PLA", PrinterId = 1 },
                new PrintProfile { Id = 2, Nom = "PETG", IsActive = true, IsDefault = false, Materiau = "PETG", PrinterId = 1 },
                new PrintProfile { Id = 3, Nom = "ABS", IsActive = false, IsDefault = false, Materiau = "ABS", PrinterId = 2 }
            };

            _profileRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(profiles);

            // Act
            var result = await _profileService.GetStatisticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalProfiles);
            Assert.Equal(2, result.ActiveProfiles);
            Assert.Equal(1, result.DefaultProfiles);
            Assert.Contains("PLA", result.CountByMateriau.Keys);
            Assert.Contains(1, result.CountByPrinter.Keys);
            Assert.Contains(2, result.CountByPrinter.Keys);
        }
    }
}
