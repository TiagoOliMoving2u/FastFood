using System.ComponentModel.DataAnnotations;

namespace FastFoodAPI.DTOs
{
    public class UpdateUserDetailsRequest
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int? RoleId { get; set; }

        [Range(0, 1)]
        public int? IsAvailable { get; set; }
    }
}
