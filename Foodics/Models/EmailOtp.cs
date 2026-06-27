namespace Foodics.Models
{
    public class EmailOtp
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Code { get; set; }

        public DateTime ExpireAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
