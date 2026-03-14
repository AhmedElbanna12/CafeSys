using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Auth
{
    public class LoginDto
    {
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
    }

}
