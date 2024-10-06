using System.ComponentModel.DataAnnotations;

namespace vega.Controllers.DTO
{
    public class AuthResponseDTO
    {
        [Required]
        public string Access { get; set; } = null!;

        [Required]
        public string Refresh { get; set; } = null!;
    }
}