namespace FastFoodDashboard.Models
{
    public class OrderDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public int AreaId { get; set; }

        public string AreaName { get; set; } = string.Empty;

        public int State { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
