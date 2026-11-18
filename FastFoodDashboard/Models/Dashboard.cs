using System.ComponentModel.DataAnnotations;

namespace FastFoodDashboard.Models
{
    public class Dashboard
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? AreaId { get; set; }
        [Range(0, 1)]
        public int IsActive { get; set; }
        public List<DashboardState> ShowStates { get; set; } = new List<DashboardState>();
        public List<DashboardPermission> PermissionsRequired { get; set; } = new List<DashboardPermission>();
    }
}
