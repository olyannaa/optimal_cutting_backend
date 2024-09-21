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

        [HttpPost]
        [Route("/token")]
        public dynamic GetToken([FromForm] AuthDto dto)
        {
            var identity = new ClaimsIdentity(new GenericIdentity(dto.Login));
            var access = _tokenManager.GetAccessToken(identity);

            return access;
        }

        [HttpGet]
        [Route("/auth")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public dynamic Index()
        {
            var currentUser = _context?.HttpContext?.User;
            return currentUser.Identity.Name;
        }
    }
}
