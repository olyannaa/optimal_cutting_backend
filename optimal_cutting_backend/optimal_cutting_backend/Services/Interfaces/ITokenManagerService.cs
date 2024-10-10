using System.Security.Claims;
using System.Security.Principal;

namespace vega.Services.Interfaces
{
    public interface ITokenManagerService
    {
        (string access, string refresh) GetTokens(IIdentity claimsIdentity);
        void DestroySessionToken();
        bool IsTokenValid();
        bool RefreshToken(out string accessToken, string refreshToken);
    }
}
