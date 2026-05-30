using System.Security.Claims;
using Backend.Interface;
using Microsoft.AspNetCore.Http;

namespace Backend.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string UserIdCacheKey = "CurrentUserService_UserId";

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public int? UserId
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return null;

                // Optimisation : Mise en cache dans le contexte de la requête 
                // pour éviter de parser la string à chaque appel dans un même cycle de vie
                if (context.Items.TryGetValue(UserIdCacheKey, out var cachedId))
                {
                    return (int?)cachedId;
                }

                // Support hybride : NameIdentifier (URI) ou "sub" (JWT standard)
                var userIdStr = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? context.User?.FindFirst("sub")?.Value;

                int? parsedId = int.TryParse(userIdStr, out var id) ? id : null;

                context.Items[UserIdCacheKey] = parsedId;
                return parsedId;
            }
        }

        public string? UserEmail
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                // Support hybride pour l'email également
                return user?.FindFirst(ClaimTypes.Email)?.Value
                       ?? user?.FindFirst("email")?.Value;
            }
        }

        public string? IpAddress
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return null;

                // Gestion des reverse proxys : vérification de l'en-tête X-Forwarded-For
                if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedHeader))
                {
                    var ipStr = forwardedHeader.FirstOrDefault();
                    if (!string.IsNullOrEmpty(ipStr))
                    {
                        // X-Forwarded-For peut contenir "client, proxy1, proxy2". On prend le premier.
                        return ipStr.Split(',')[0].Trim();
                    }
                }

                return context.Connection.RemoteIpAddress?.ToString();
            }
        }
    }
}