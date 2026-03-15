using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class UserPoints
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int TotalPoints { get; set; }

        public int UsedPoints { get; set; }
    }
}
