namespace Backend.Interface
{
    public interface IAuthMailSender
    {
        Task SendPasswordResetAsync(string toEmail, string userPrenom, string resetUrl);
    }
}
