using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Auth
{
    public class ResendOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
