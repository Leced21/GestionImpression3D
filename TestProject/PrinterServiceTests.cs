using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Moq;

namespace TestProject
{
    public class PrinterServiceTests
    {
        private readonly Mock<IPrinterRepository> _printerRepositoryMock = new();
        private readonly Mock<IAuditLogger> _auditLoggerMock = new();
        private readonly PrinterService _service;

        public PrinterServiceTests()
        {
            _service = new PrinterService(_printerRepositoryMock.Object, _auditLoggerMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WithPowerWatts_ReturnsPowerWatts()
        {
            var request = new CreatePrinterRequest
            {
                Nom = "Bambu Lab A1",
                Reference = "PRN-001",
                Model = "A1",
                Brand = "Bambu Lab",
                Type = "FDM",
                MaxPrintSizeX = 256,
                MaxPrintSizeY = 256,
                MaxPrintSizeZ = 256,
                PowerWatts = 180m
            };

            _printerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Printer>()))
                .ReturnsAsync((Printer printer) => printer);

            var result = await _service.CreateAsync(request);

            Assert.Equal(180m, result.PowerWatts);
        }

        [Fact]
        public async Task UpdateAsync_WithPowerWatts_UpdatesPrinterDetails()
        {
            var printer = Printer.Create("Old", "OLD-001", "MK3", "Prusa", PrinterType.FDM, "", 210, 210, 210, 120m);
            var request = new UpdatePrinterRequest
            {
                Nom = "Prusa MK4",
                Reference = "PRN-002",
                Model = "MK4",
                Brand = "Prusa",
                Type = "FDM",
                IpAddress = "192.168.1.42",
                MaxPrintSizeX = 250,
                MaxPrintSizeY = 210,
                MaxPrintSizeZ = 220,
                PowerWatts = 135m,
                IsActive = true
            };

            _printerRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(printer);
            _printerRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Printer>()))
                .ReturnsAsync((Printer updated) => updated);

            var result = await _service.UpdateAsync(1, request);

            Assert.NotNull(result);
            Assert.Equal("Prusa MK4", result.Nom);
            Assert.Equal("PRN-002", result.Reference);
            Assert.Equal(135m, result.PowerWatts);
            Assert.Equal(250, result.MaxPrintSizeX);
        }

        [Fact]
        public async Task CreateAsync_WithInvalidPowerWatts_ThrowsArgumentException()
        {
            var request = new CreatePrinterRequest
            {
                Nom = "Printer",
                Reference = "PRN-003",
                Model = "Model",
                Brand = "Brand",
                Type = "FDM",
                MaxPrintSizeX = 210,
                MaxPrintSizeY = 210,
                MaxPrintSizeZ = 210,
                PowerWatts = 0m
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
            _printerRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Printer>()), Times.Never);
        }
    }
}
