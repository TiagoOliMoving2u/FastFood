using FastFoodAPI.Models;

namespace FastFoodAPI.DTOs
{
    public class StateHistoryResponse
    {
        int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        public OrderState? OldState { get; set; }
        public OrderState NewState { get; set; }
        public DateTime ChangedOn { get; set; }
    }
}
