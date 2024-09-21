using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace vega.Services
{
    public class TokenManagerService : ITokenManagerService
    {
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _context;

        public TokenManagerService(IConfiguration config,
            IDistributedCache distributedCache, IHttpContextAccessor httpContext)
        { 
            _config = config;
            _cache = distributedCache;
            _context = httpContext;
        }

        public string GetAccessToken(ClaimsIdentity identity)
        {
            var handler = new JwtSecurityTokenHandler();

            var authOptions = _config.GetSection("AuthOptions");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions["Key"]));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = handler.CreateJwtSecurityToken(
                subject: identity,
                signingCredentials: signingCredentials,
                audience: authOptions["Audience"],
                issuer: authOptions["Issuer"],
                expires: DateTime.UtcNow.AddMinutes(30));

            return handler.WriteToken(token);
        }

        public void DestroySessionToken()
        {
            var authHeader = GetCurrent();
            var securityToken = new JwtSecurityToken(authHeader);
            _cache.Set(authHeader, new byte[1] { 1 }, options: new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = securityToken.ValidTo - DateTime.UtcNow
            });
        }

        public bool IsTokenValid()
        {
            var authHeader = GetCurrent();
            if (authHeader == null)
                return false;

            var cachedToken = _cache.Get(authHeader);

            return cachedToken == null;
        }

        private string? GetCurrent()
        {
            string? authorizationHeader = _context
            ?.HttpContext?.Request.Headers["authorization"];

            return authorizationHeader == null
                ? null
                : authorizationHeader.Split(" ").Last();
        }
    }
}
