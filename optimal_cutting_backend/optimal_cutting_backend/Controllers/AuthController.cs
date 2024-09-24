using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using vega.Controllers.DTO;
using vega.Migrations.EF;
using Npgsql;
using System.Security.Cryptography;
using vega.Services;

namespace vega.Controllers
{
    [Route("/[controller]")]
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

        [HttpPost]
        [Route("/token")]
        public dynamic GetToken([FromForm] AuthDto dto)
        {
            var identity = new ClaimsIdentity(new GenericIdentity(dto.Login));
            var access = _tokenManager.GetAccessToken(identity);

            return access;
        }

        [HttpGet]
        [Route("/user")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public dynamic Index()
        {
            var currentUser = _context?.HttpContext?.User;
            return currentUser.Identity.Name;
        }

        [HttpPost]
        [Route("/user/validPassword")]
        public IActionResult ValidPassword(string login, string password)
        {
            var user = _db.Users.FirstOrDefault(x => x.Login == login);
            if (user == null) return BadRequest("user is not found");
            var encryptePassword = CalculateSHA256(password);
            if (encryptePassword == user.Password) return Ok(true);
            return Ok(false);

        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public string CalculateSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
