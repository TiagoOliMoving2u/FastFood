namespace FastFoodAPI.DTOs
{
    public class CreateOrderItemRequest
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
