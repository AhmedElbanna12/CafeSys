namespace Foodics.Dtos.SplashScreen
{
    public class SplashScreenDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Photo { get; set; } = null!;

        public string? Video { get; set; }
    }
}
