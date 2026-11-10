namespace FastFoodDashboard.Models
{
    public class RolePermissionsResponse
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }
}
