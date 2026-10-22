
namespace FastFoodAPI.DTOs
{

    public class CreateOrderRequest         //Data sent by client
    {
        public int AreaId { get; set; }
        public ICollection<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();
        public decimal TotalPrice { get; set; }
    }
}
