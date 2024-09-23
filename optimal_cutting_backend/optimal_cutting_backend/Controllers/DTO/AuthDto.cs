using System.ComponentModel.DataAnnotations;

namespace vega.Controllers.DTO
{
    public class AuthDto
    {
        [Required]
        public string Login { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
