using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Auth
{
    public class VerifyEmailDto
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        public string Otp { get; set; }

    }
}
