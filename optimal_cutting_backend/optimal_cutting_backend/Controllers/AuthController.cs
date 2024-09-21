using Microsoft.AspNetCore.Mvc;
using vega.Migrations.EF;

namespace vega.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly VegaContext _db;

        public AuthController(ILogger<AuthController> logger, VegaContext context)
        {
            _logger = logger;
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
