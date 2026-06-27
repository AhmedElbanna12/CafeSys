using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class ProductSize
    {
        [Key]
        public int Id { get; set; }

        public string? NameAr { get; set; }
        public string? NameEn { get; set; }

        public decimal Price { get; set; }

        public bool IsDefault { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product Product { get; set; }


    }
}
