using FastFoodAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace FastFoodAPI.DTOs
{
    public class CreateDashboardRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? AreaId { get; set; }
        [Range(0, 1)]
        public int IsArea { get; set; }
        [Range(0, 1)]
        public List<DashboardState> ShowStates { get; set; } = new List<DashboardState>();
        public List<DashboardPermission> PermissionsRequired { get; set; } = new List<DashboardPermission>();
    }
}
