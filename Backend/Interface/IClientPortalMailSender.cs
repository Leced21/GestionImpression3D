namespace Backend.Interface
{
    public interface IClientPortalMailSender
    {
        Task SendMagicLinkAsync(string toEmail, string clientNom, string magicLinkUrl);
    }
}
