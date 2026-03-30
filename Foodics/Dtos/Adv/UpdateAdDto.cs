namespace Foodics.Dtos.Adv
{
    public class UpdateAdDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile? Image { get; set; } // optional
    }
}
