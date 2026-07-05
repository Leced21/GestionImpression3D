namespace Backend.Interface
{
    public interface IEmailSender
    {
        Task SendHtmlAsync(string toEmail, string subject, string htmlBody);
    }
}
