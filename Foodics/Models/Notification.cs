namespace Foodics.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string ?  UserId { get; set; } // optional لكل مستخدم


        public string? TitleAr { get; set; }

        public string? TitleEn { get; set; }

        public string? BodyAr { get; set; }

        public string? BodyEn { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
