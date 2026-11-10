namespace FastFoodAPI.DTOs
{
    public class UpdateRolePermissionsRequest
    {
        public List<int> PermissionIds { get; set; } = new();
    }
}
