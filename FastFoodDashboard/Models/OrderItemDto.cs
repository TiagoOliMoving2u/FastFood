namespace FastFoodDashboard.Models
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public override string ToString()
        {
            return Quantity + " x " + Name;
        }
    }

}
