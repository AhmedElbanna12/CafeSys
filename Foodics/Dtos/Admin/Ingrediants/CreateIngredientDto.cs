namespace Foodics.Dtos.Admin.Ingrediants
{
    public class CreateIngredientDto
    {
        public string Name { get; set; }

        public string Unit { get; set; }

        public decimal Quantity { get; set; }

        public decimal MinQuantity { get; set; }
    }
}
