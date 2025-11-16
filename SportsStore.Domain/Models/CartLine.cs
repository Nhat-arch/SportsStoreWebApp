namespace SportsStore.Domain.Models
{
    public class CartLine
    {
        public int CartLineID { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
    }
}