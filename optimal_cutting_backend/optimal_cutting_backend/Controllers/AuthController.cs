using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;
using vega.Controllers.DTO;
using vega.Migrations.EF;
using System.Security.Cryptography;
using vega.Services.Interfaces;
using System.Text;

namespace vega.Controllers
{
    [Route("/[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenManagerService _tokenManager;
        private readonly IHttpContextAccessor _context;
        private readonly VegaContext _db;

        public AuthController(ILogger<AuthController> logger,
            ITokenManagerService tokenManager, IHttpContextAccessor httpContext, VegaContext dbContext)
        {
            _logger = logger;
            _tokenManager = tokenManager;
            _context = httpContext;
            _db = dbContext;
        }

        /// <summary>
        /// Authorizes user in system.
        /// </summary>
        /// <returns>Returns JWT</returns>
        /// <response code="200">Returns JWT access and refresh</response>
        /// <response code="400">If user is not registered in system or password is wrong</response>
        [HttpPost]
        [Route("/login")]
        public IActionResult GetTokens([FromForm] AuthDTO dto)
        {
            var user = _db.Users.FirstOrDefault(x => x.Login == dto.Login);
            if (user == null) return BadRequest("user is not found");

            var encryptePassword = CalculateSHA256(dto.Password);
            if (encryptePassword != user.Password) return BadRequest("wrong password");

            var identity = new ClaimsIdentity(new GenericIdentity(user.FullName));
            var tokens = _tokenManager.GetTokens(identity);

            return Ok(new AuthResponseDTO() {
                Refresh = tokens.refresh,
                Access = tokens.access}
            );
        }

        /// <summary>
        /// Deactivates user's access-token and logs out user from system.
        /// </summary>
        /// <returns>User name</returns>
        /// <response code="200">Logout user succefully</response>
        [HttpGet]
        [Route("/logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DestroySessionTokens()
        {
            _tokenManager.DestroySessionToken();
            return Ok();   
        }

        /// <summary>
        /// Retrieves logged in user information.
        /// </summary>
        /// <returns>User name</returns>
        /// <response code="200">Returns user info</response>
        /// <response code="401">Not authorized</response>
        [HttpGet]
        [Route("/user")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUserIdentity()
        {
            var currentUser = _context?.HttpContext?.User;
            return Ok(currentUser.Identity.Name);
        }

        /// <summary>
        /// Refreshes user access token via refresh token.
        /// </summary>
        /// <returns>Returns JWT</returns>
        /// <response code="200">Returns JWT access and refresh</response>
        /// <response code="403">Refresh has expired or not associated with user's access</response>
        [HttpPost]
        [Route("/refresh-token")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult RefreshAccessToken([FromForm] string refreshToken)
        {
            if(_tokenManager.RefreshToken(out var accessToken, refreshToken))
            {
                return Ok(new AuthResponseDTO() {
                    Refresh = refreshToken,
                    Access = accessToken}
                    );
            }

            return Forbid();
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
