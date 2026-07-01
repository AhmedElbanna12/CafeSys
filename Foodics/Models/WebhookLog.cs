namespace Foodics.Models
{
    public class WebhookLog
    {
        public int Id { get; set; }

        public string Body { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
