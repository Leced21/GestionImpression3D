using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Moq;

namespace TestProject
{
    public class PrintIncidentServiceTests
    {
        private readonly Mock<IPrintIncidentRepository> _incidentRepositoryMock = new();
        private readonly Mock<ICurrentUserService> _currentUserMock = new();
        private readonly Mock<IAuditLogger> _auditLoggerMock = new();
        private readonly IPrintIncidentService _service;

        public PrintIncidentServiceTests()
        {
            _service = new PrintIncidentService(
                _incidentRepositoryMock.Object,
                _currentUserMock.Object,
                _auditLoggerMock.Object
            );
        }

        [Fact]
        public async Task ResolveAsync_WithoutCurrentUser_ThrowsInvalidOperationException()
        {
            var incident = new PrintIncident
            {
                Id = 7,
                Title = "Buse bouchée",
                Status = IncidentStatus.Ouvert
            };

            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _currentUserMock.SetupGet(x => x.UserId).Returns((int?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.ResolveAsync(incident.Id, new ResolveIncidentRequest { Resolution = "Nettoyage effectué" })
            );

            _incidentRepositoryMock.Verify(x => x.ResolveAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResolveAsync_WithCurrentUser_ResolvesAndLogsStatusChange()
        {
            var incident = new PrintIncident
            {
                Id = 8,
                Title = "Plateau déréglé",
                Status = IncidentStatus.EnCours
            };
            var resolvedIncident = new PrintIncident
            {
                Id = 8,
                Title = "Plateau déréglé",
                Status = IncidentStatus.Résolu,
                Resolution = "Calibration effectuée",
                ResolvedBy = 42
            };

            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _currentUserMock.SetupGet(x => x.UserId).Returns(42);
            _incidentRepositoryMock
                .Setup(x => x.ResolveAsync(incident.Id, "Calibration effectuée", 42))
                .ReturnsAsync(resolvedIncident);

            var result = await _service.ResolveAsync(incident.Id, new ResolveIncidentRequest { Resolution = "Calibration effectuée" });

            Assert.NotNull(result);
            Assert.Equal(IncidentStatus.Résolu, result.Status);
            Assert.Equal(42, result.ResolvedBy);
            _incidentRepositoryMock.Verify(x => x.ResolveAsync(incident.Id, "Calibration effectuée", 42), Times.Once);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(EntityType.PrintIncident, incident.Id, "EnCours", "Résolu"), Times.Once);
        }

        [Fact]
        public async Task ResolveAsync_WithNonExistingIncident_ReturnsNull()
        {
            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((PrintIncident?)null);

            var result = await _service.ResolveAsync(99, new ResolveIncidentRequest { Resolution = "N/A" });

            Assert.Null(result);
            _incidentRepositoryMock.Verify(x => x.ResolveAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_CreatesIncidentWithReportedByFromCurrentUser()
        {
            var request = new CreateIncidentRequest
            {
                PrintJobId = 3,
                PrinterId = 2,
                Title = "Bourrage filament",
                Description = "Le filament ne s'extrude plus",
                Severity = IncidentSeverity.Haute
            };
            var created = new PrintIncident { Id = 10, Title = request.Title, Status = IncidentStatus.Ouvert, ReportedBy = 7 };

            _currentUserMock.SetupGet(x => x.UserId).Returns(7);
            _incidentRepositoryMock.Setup(x => x.CreateAsync(It.Is<PrintIncident>(i =>
                i.Title == request.Title && i.ReportedBy == 7 && i.Status == IncidentStatus.Ouvert
            ))).ReturnsAsync(created);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            _auditLoggerMock.Verify(x => x.LogCreationAsync(EntityType.PrintIncident, 10, request.Title), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingIncident_ReturnsTrueAndLogsDeletion()
        {
            var incident = new PrintIncident { Id = 1, Title = "Incident" };
            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(incident);
            _incidentRepositoryMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _service.DeleteAsync(1);

            Assert.True(result);
            _auditLoggerMock.Verify(x => x.LogDeletionAsync(EntityType.PrintIncident, 1, "Incident"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingIncident_ReturnsFalse()
        {
            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((PrintIncident?)null);

            var result = await _service.DeleteAsync(99);

            Assert.False(result);
            _incidentRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
            _auditLoggerMock.Verify(x => x.LogDeletionAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryDeleteFails_DoesNotLogDeletion()
        {
            var incident = new PrintIncident { Id = 1, Title = "Incident" };
            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(incident);
            _incidentRepositoryMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(false);

            var result = await _service.DeleteAsync(1);

            Assert.False(result);
            _auditLoggerMock.Verify(x => x.LogDeletionAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllIncidents()
        {
            var incidents = new List<PrintIncident>
            {
                new PrintIncident { Id = 1, Title = "Incident 1" },
                new PrintIncident { Id = 2, Title = "Incident 2" }
            };
            _incidentRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(incidents);

            var result = await _service.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ExistingIncident_ReturnsIncident()
        {
            var incident = new PrintIncident { Id = 1, Title = "Incident" };
            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(incident);

            var result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingIncident_ReturnsNull()
        {
            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((PrintIncident?)null);

            var result = await _service.GetByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByPrinterAsync_ReturnsIncidentsForPrinter()
        {
            var incidents = new List<PrintIncident>
            {
                new PrintIncident { Id = 1, PrinterId = 5, Title = "Incident" }
            };
            _incidentRepositoryMock.Setup(x => x.GetByPrinterAsync(5)).ReturnsAsync(incidents);

            var result = await _service.GetByPrinterAsync(5);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByPrintJobAsync_ReturnsIncidentsForPrintJob()
        {
            var incidents = new List<PrintIncident>
            {
                new PrintIncident { Id = 1, PrintJobId = 3, Title = "Incident" }
            };
            _incidentRepositoryMock.Setup(x => x.GetByPrintJobAsync(3)).ReturnsAsync(incidents);

            var result = await _service.GetByPrintJobAsync(3);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetStatisticsAsync_DelegatesToRepositoryWithDateRange()
        {
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 6, 30);
            var stats = new IncidentStatisticsDto { TotalIncidents = 5, OpenIncidents = 2 };

            _incidentRepositoryMock.Setup(x => x.GetStatisticsAsync(start, end)).ReturnsAsync(stats);

            var result = await _service.GetStatisticsAsync(start, end);

            Assert.Equal(5, result.TotalIncidents);
            Assert.Equal(2, result.OpenIncidents);
        }

        [Fact]
        public async Task UpdateStatusAsync_ExistingIncident_UpdatesStatusAndLogsChange()
        {
            var incident = new PrintIncident { Id = 1, Title = "Incident", Status = IncidentStatus.Ouvert };
            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(incident);
            _incidentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<PrintIncident>()))
                .ReturnsAsync((PrintIncident i) => i);

            var result = await _service.UpdateStatusAsync(1, IncidentStatus.EnCours);

            Assert.NotNull(result);
            Assert.Equal(IncidentStatus.EnCours, result.Status);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(EntityType.PrintIncident, 1, "Ouvert", "EnCours"), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_NonExistingIncident_ReturnsNull()
        {
            _incidentRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((PrintIncident?)null);

            var result = await _service.UpdateStatusAsync(99, IncidentStatus.Fermé);

            Assert.Null(result);
            _incidentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PrintIncident>()), Times.Never);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
