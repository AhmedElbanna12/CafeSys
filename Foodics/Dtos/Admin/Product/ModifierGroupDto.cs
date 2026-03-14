namespace Foodics.Dtos.Admin.Product
{
    public class ModifierGroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public int MaxSelections { get; set; }

        public List<ModifierOptionDto> Options { get; set; }
    }
}
