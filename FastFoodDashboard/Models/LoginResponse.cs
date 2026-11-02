
namespace FastFoodDashboard.Models
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = "";
        public string TokenType { get; set; } = "";
        public DateTime ExpiresAt { get; set; }

    }
}
