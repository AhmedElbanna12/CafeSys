namespace Foodics.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string ?  UserId { get; set; } // optional لكل مستخدم
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
