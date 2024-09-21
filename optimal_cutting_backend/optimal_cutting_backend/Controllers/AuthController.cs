using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using vega.Migrations.EF;
using vega.Services;

namespace vega.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenManagerService _tokenManager;
        private readonly IHttpContextAccessor _context;
        private readonly VegaContext _db;

        public AuthController(ILogger<AuthController> logger, ITokenManagerService tokenManager, IHttpContextAccessor httpContext, VegaContext dbContext)
        {
            _logger = logger;
            _tokenManager = tokenManager;
            _context = httpContext;
            _db = dbContext;
        }

        [HttpGet]
        [Route("/token")]
        public dynamic GetToken()
        {
            var handler = new JwtSecurityTokenHandler();

            var sec = "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sec));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[] { new Claim("user_id", "57dc51a3389b30fed1b13f91") };
            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials,
                audience: "Test",
                issuer: "Test",
                expires: DateTime.UtcNow.AddMinutes(30));
            return handler.WriteToken(token);
        }

        [HttpGet]
        [Route("/auth")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public dynamic Index()
        {
            var currentUser = _context.HttpContext.User;
            return currentUser.Identity.Name;
        }
    }
}
