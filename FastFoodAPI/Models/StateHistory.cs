namespace FastFoodAPI.Models
{
    public class StateHistory
    {
        int Id { get; set; }
        public int OrderId { get; set; }
        public OrderState OldState { get; set; }
        public OrderState NewState { get; set; }
        public DateTime ChangedOn { get; set; }

    }
}
