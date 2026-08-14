namespace Foodics.Models
{
    public class PointsSettings
    {
        public int Id { get; set; }

        public decimal EgpPerPoint { get; set; } = 20m;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
