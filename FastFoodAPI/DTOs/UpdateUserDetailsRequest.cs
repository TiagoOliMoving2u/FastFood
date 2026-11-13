namespace FastFoodAPI.DTOs
{
    public class UpdateUserDetailsRequest
    {
        public int UserId { get; set; }
        public int? RoleId { get; set; } = null;
        public int? IsAvailable { get; set; } = null;
    }
}
