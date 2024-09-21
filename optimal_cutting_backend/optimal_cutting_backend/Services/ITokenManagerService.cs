using System.Security.Claims;

namespace vega.Services
{
    public interface ITokenManagerService
    {
        string GetAccessToken(ClaimsIdentity identity);
        string DestroyToken();
    }
}
