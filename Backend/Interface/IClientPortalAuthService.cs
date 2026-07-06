using Backend.DTOs;

namespace Backend.Interface
{
    public interface IClientPortalAuthService
    {
        Task RequestAccessAsync(string email);
        Task<ClientPortalAuthResponse?> ConsumeAsync(string rawToken);
    }
}
