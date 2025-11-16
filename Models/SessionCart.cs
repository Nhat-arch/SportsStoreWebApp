using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SportsStore.Domain.Models;
using SportsStoreWebApp.Extensions;
namespace SportsStoreWebApp.Models
{
    public class SessionCart : Cart
    {
        public static Cart GetCart(IServiceProvider services)
        {
            ISession? session =
           services.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.Session
           ;
            SessionCart cart =
           session?.GetObjectFromJson<SessionCart>("Cart") ?? new SessionCart();
            cart.Session = session!;
            return cart;
        }
        [JsonIgnore]
        public ISession Session { get; set; } = null!;
        public override void AddItem(Product product, int quantity)
        {
            base.AddItem(product, quantity);
            Session?.SetObjectAsJson("Cart", this);
        }
        public override void RemoveItem(Product product)
        {
            base.RemoveItem(product);
            Session?.SetObjectAsJson("Cart", this);
        }
        public override void Clear()
        {
            base.Clear();
            Session?.Remove("Cart");
        }
    }
}