namespace FastFoodAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public DateTime CreatedOn = DateTime.UtcNow;
        public OrderState State { get; set; } = OrderState.AlredyOrdered;
        public int AreaId { get; set; }
        public Area Area { get; set; } = null!;
    }
}
