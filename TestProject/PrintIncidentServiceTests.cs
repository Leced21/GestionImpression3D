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

            var service = new PrintIncidentService(
                _incidentRepositoryMock.Object,
                _currentUserMock.Object,
                _auditLoggerMock.Object
            );

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ResolveAsync(incident.Id, new ResolveIncidentRequest { Resolution = "Nettoyage effectué" })
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

            var service = new PrintIncidentService(
                _incidentRepositoryMock.Object,
                _currentUserMock.Object,
                _auditLoggerMock.Object
            );

            var result = await service.ResolveAsync(incident.Id, new ResolveIncidentRequest { Resolution = "Calibration effectuée" });

            Assert.NotNull(result);
            Assert.Equal(IncidentStatus.Résolu, result.Status);
            Assert.Equal(42, result.ResolvedBy);
            _incidentRepositoryMock.Verify(x => x.ResolveAsync(incident.Id, "Calibration effectuée", 42), Times.Once);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(EntityType.PrintIncident, incident.Id, "EnCours", "Résolu"), Times.Once);
        }
    }
}
