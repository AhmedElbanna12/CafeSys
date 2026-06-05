using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class ModifierGroup
    {
        [Key]
        public int Id { get; set; }

        public string? NameAr { get; set; }

        public string? NameEn { get; set; }

        public bool IsRequired { get; set; }

        public int MaxSelections { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        public ICollection<ModifierOption> Options { get; set; }
    }
}
