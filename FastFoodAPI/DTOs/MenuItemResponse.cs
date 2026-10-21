namespace FastFoodAPI.DTOs
{
    public class MenuItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
        public int AvailableUnits { get; set; }
    }
}
