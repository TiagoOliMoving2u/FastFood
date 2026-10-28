namespace FastFoodDashboard.Models
{
    public class DashboardConfig
    {
        public string Title { get; set; } = "";

        public int? AreaId { get; set; }

        public int[] VisibleStates { get; set; } = [];

        public bool ShowAreaName { get; set; }
    }
}
