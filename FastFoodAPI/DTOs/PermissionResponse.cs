namespace FastFoodAPI.DTOs
{
    //this DTO represent each permission each role can have
    public class PermissionResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
