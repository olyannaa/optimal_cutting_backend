using Microsoft.AspNetCore.Mvc;
using vega.Migrations.EF;
using vega.Services;

namespace vega.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenManagerService _tokenManager;
        private readonly VegaContext _db;

        public AuthController(ILogger<AuthController> logger, ITokenManagerService tokenManager, VegaContext context)
        {
            _logger = logger;
            _tokenManager = tokenManager;
            _db = context;
        }

        [HttpGet]
        [Route("/user")]
        public IActionResult Index()
        {
            return Ok(_db.Users.ToList());
        }
    }
}
