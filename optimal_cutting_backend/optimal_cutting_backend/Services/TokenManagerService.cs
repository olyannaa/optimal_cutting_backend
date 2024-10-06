using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
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

        public (string, string) GetTokens(IIdentity claimsIdentity)
        {
            var accessToken = GenerateAccessToken(claimsIdentity);
            var refreshToken = GenerateRefreshToken();

            _cache.SetString(refreshToken, accessToken, options: new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });

            return (accessToken, refreshToken);
        }

        public void DestroySessionToken()
        {
            var authHeader = GetCurrent();
            var securityToken = new JwtSecurityToken(authHeader);
            _cache.SetString(authHeader, authHeader, options: new DistributedCacheEntryOptions()
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

        public bool RefreshToken(out string accessToken, string refreshToken)
        {
            var oldAccessToken = _cache.GetString(refreshToken);
            if (oldAccessToken == null)
            {
                accessToken = String.Empty;
                return false;
            }

            if (oldAccessToken != GetCurrent())
            {
                accessToken = String.Empty;
                return false;
            }

            var identity = _context?.HttpContext?.User?.Identity;
            accessToken = GenerateAccessToken(identity);

            _cache.Remove(refreshToken);
            _cache.SetString(refreshToken, accessToken, options: new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });
            DestroySessionToken();

            return true;
        }

        private string? GetCurrent()
        {
            string? authorizationHeader = _context
            ?.HttpContext?.Request.Headers["authorization"];

            return authorizationHeader == null
                ? null
                : authorizationHeader.Split(" ").Last();
        }

        private string GenerateAccessToken(IIdentity identity)
        {
            var handler = new JwtSecurityTokenHandler();

            var authOptions = _config.GetSection("AuthOptions");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions["Key"]));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var secToken = handler.CreateJwtSecurityToken(
                subject: identity as ClaimsIdentity,
                signingCredentials: signingCredentials,
                audience: authOptions["Audience"],
                issuer: authOptions["Issuer"],
                expires: DateTime.UtcNow.AddMinutes(30));

            return handler.WriteToken(secToken);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                var refreshToken = Convert.ToBase64String(randomNumber);
                return refreshToken;
            }
        }
    }
}
