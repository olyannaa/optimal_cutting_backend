using System.Security.Claims;

namespace vega.Services.Interfaces
{
    public interface ITokenManagerService
    {
        string GetAccessToken(ClaimsIdentity identity);
        string DestroyToken();
    }
}
