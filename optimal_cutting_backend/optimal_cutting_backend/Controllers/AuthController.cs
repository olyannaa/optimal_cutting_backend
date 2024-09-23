using Microsoft.AspNetCore.Mvc;
using vega.Migrations.EF;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

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
