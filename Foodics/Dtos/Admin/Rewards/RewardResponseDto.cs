namespace Foodics.Dtos.Admin.Rewards
{
    public class RewardResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PointsRequired { get; set; }
        public int ? ProductId { get; set; }  
        public bool IsActive { get; set; }
    }
}
