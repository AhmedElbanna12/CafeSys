using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; }
    }
}
