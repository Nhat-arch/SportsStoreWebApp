using System.Collections.Generic;
using System.Linq;
namespace SportsStore.Domain.Models
{
    public class Cart
    {
        public List<CartLine> Lines { get; set; } = new();
        public virtual void AddItem(Product product, int quantity)
        {
            var line = Lines.FirstOrDefault(p => p.Product.ProductID == product.ProductID);
            if (line == null)
                Lines.Add(new CartLine
                {
                    Product = product,
                    Quantity = quantity
                });
            else
                line.Quantity += quantity;
        }
        public virtual void RemoveItem(Product product) =>
        Lines.RemoveAll(l => l.Product.ProductID == product.ProductID);
        public virtual void Clear() => Lines.Clear();
        public decimal ComputeTotalValue() => Lines.Sum(e => e.Product.Price * e.Quantity);
    }
}