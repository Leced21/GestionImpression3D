using Backend.Interface;
using Backend.Models;
using Backend.Options;
using Backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace TestProject
{
    public class MailingTests
    {
        [Theory]
        [InlineData("", 587, "from@printflow3d.com", "Mail:Smtp:Host")]
        [InlineData("smtp.example.com", 0, "from@printflow3d.com", "Mail:Smtp:Port")]
        [InlineData("smtp.example.com", 70000, "from@printflow3d.com", "Mail:Smtp:Port")]
        [InlineData("smtp.example.com", 587, "", "Mail:Smtp:FromEmail")]
        public async Task SmtpEmailSender_WithInvalidConfiguration_ThrowsBeforeSending(
            string host, int port, string fromEmail, string expectedMessageFragment)
        {
            var options = Options.Create(new SmtpOptions { Host = host, Port = port, FromEmail = fromEmail });
            var sender = new SmtpEmailSender(options, NullLogger<SmtpEmailSender>.Instance);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sender.SendHtmlAsync("client@example.com", "Sujet", "<p>Corps</p>"));

            Assert.Contains(expectedMessageFragment, ex.Message);
        }

        [Fact]
        public async Task LoggingEmailSender_DoesNotThrowWithoutSmtpConfigured()
        {
            var sender = new LoggingEmailSender(NullLogger<LoggingEmailSender>.Instance);

            await sender.SendHtmlAsync("client@example.com", "Sujet", "<p>Corps</p>");
        }

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
