using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class TokenManagerService : ITokenManagerService
    {
        private readonly IConfiguration _config;
        public TokenManagerService(IConfiguration config)
        { 
            _config = config;
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

        public string DestroyToken()
        {
            throw new NotImplementedException();
        }
    }
}
