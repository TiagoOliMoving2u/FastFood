using FastFoodAPI.Models;

namespace FastFoodAPI.DTOs
{
    public class OrderResponse      //Data sent to client by API
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public int AreaId { get; set; }
        public string AreaName { get; set; } = "";
        public OrderState State { get; set; }
        public DateTime CreatedOn { get; set; } 
    }
}
