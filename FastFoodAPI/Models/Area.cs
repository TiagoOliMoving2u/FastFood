namespace FastFoodAPI.Models
{
    //This class represents the area of the restaurant,
    //Ex: id=1, Name=Esplanada, Acronym=E
    //And the Area can have a lot of orders, soo the relation is one for many (1:n)
    public class Area
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Acronym { get; set; } = "";  // "Sigla"
        public int LastOrderNumber { get; set; } = 0;
        public ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}
