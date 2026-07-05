using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace TestProject
{
    public class MailingTests
    {
        [Fact]
        public async Task ClientPortalMailSender_SendsMagicLinkWithoutInjectingRawHtml()
        {
            var emailSender = new Mock<IEmailSender>();
            var sender = new ClientPortalMailSender(emailSender.Object);

            await sender.SendMagicLinkAsync(
                "client@example.com",
                "<script>alert('x')</script>",
                "https://app.example.com/portail/acces?token=abc");

            emailSender.Verify(x => x.SendHtmlAsync(
                "client@example.com",
                "Votre accès au portail PrintFlow3D",
                It.Is<string>(body =>
                    body.Contains("https://app.example.com/portail/acces?token=abc") &&
                    body.Contains("&lt;script&gt;") &&
                    !body.Contains("<script>"))), Times.Once);
        }

        [Fact]
        public async Task CreateInvitation_SendsEmailWithConfiguredFrontendUrl()
        {
            var invitationRepository = CreateInvitationRepository();
            var emailSender = new Mock<IEmailSender>();
            var service = CreateService(invitationRepository.Object, emailSender.Object);

            var invitation = await service.CreateInvitationAsync("user@example.com", "Operator", 1);

            Assert.Equal(42, invitation.Id);
            emailSender.Verify(x => x.SendHtmlAsync(
                "user@example.com",
                "Invitation à rejoindre PrintFlow3D",
                It.Is<string>(body => body.Contains("https://app.example.com/accept-invitation?token="))), Times.Once);
            invitationRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateInvitation_WhenEmailFails_DeletesCreatedInvitation()
        {
            var invitationRepository = CreateInvitationRepository();
            var emailSender = new Mock<IEmailSender>();
            emailSender
                .Setup(x => x.SendHtmlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("SMTP indisponible"));
            var service = CreateService(invitationRepository.Object, emailSender.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateInvitationAsync("user@example.com", "Operator", 1));

            invitationRepository.Verify(x => x.DeleteAsync(42), Times.Once);
        }

        private static Mock<IInvitationRepository> CreateInvitationRepository()
        {
            var repository = new Mock<IInvitationRepository>();
            repository
                .Setup(x => x.CreateAsync(It.IsAny<Invitation>()))
                .ReturnsAsync((Invitation invitation) =>
                {
                    invitation.Id = 42;
                    return invitation;
                });
            repository.Setup(x => x.DeleteAsync(It.IsAny<int>())).ReturnsAsync(true);
            return repository;
        }

        private static InvitationService CreateService(
            IInvitationRepository invitationRepository,
            IEmailSender emailSender)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Frontend:BaseUrl"] = "https://app.example.com"
                })
                .Build();

            return new InvitationService(
                invitationRepository,
                Mock.Of<IUserRepository>(),
                emailSender,
                configuration);
        }
    }
}
