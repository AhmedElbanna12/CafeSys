using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class UserDevice
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public string DeviceToken { get; set; } = null!;

        // (اختياري بس مهم)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
