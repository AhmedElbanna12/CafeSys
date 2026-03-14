namespace Foodics.Dtos.Admin.Product
{
    public class CreateModifierGroupDto
    {
        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public int MaxSelections { get; set; }
    }
}
