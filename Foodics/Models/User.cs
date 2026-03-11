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
    }
}
