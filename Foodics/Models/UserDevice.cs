namespace Foodics.Models
{
    public class UserDevice
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string DeviceToken { get; set; }

        // (اختياري بس مهم)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
