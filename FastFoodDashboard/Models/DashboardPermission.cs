namespace FastFoodDashboard.Models
{
    public class DashboardPermission
    {
        public int Id { get; set; }
        public int DashboardId { get; set; }
        public int PermissionId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
