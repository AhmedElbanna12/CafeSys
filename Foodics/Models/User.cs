using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;


        [Phone]
        [PersonalData]
        public override string PhoneNumber { get; set; } = null!;

        public string CustomerCode { get; set; } = Guid.NewGuid().ToString();



        public string ? RefreshToken { get; set; }
        public DateTime  ? RefreshTokenExpiryTime { get; set; }


        public bool IsBlocked { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public DateTime? BlockedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
