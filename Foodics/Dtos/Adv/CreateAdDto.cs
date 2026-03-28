namespace Foodics.Dtos.Adv
{
    public class CreateAdDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile Image { get; set; } = default!;
    }
}
