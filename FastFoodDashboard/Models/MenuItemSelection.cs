namespace FastFoodDashboard.Models
{
    public class MenuItemSelection
    {
        public MenuItemDto Item { get; set; } = new();
        public int Quantity { get; set; }
    }
}
