using Microsoft.AspNetCore.Mvc;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Models; // Giả định Cart và CartItem ở đây
using SportsStoreWebApp.Extensions; // Giả định IProductRepository ở đây
// Cần truy cập Session để lấy giỏ hàng, nên sẽ tiêm IHttpContextAccessor hoặc trực tiếp từ HttpContext.Session
// Để đơn giản, giả định Cart được lưu trong Session và có một extension method GetCart()
public class CartSummaryViewComponent : ViewComponent
{
    private IHttpContextAccessor _httpContextAccessor;
    private IProductRepository _productRepository; // Để lấy Product nếu cần
    public CartSummaryViewComponent(IHttpContextAccessor httpContextAccessor, IProductRepository productRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _productRepository = productRepository;
    }
    public IViewComponentResult Invoke()
    {
        // Lấy giỏ hàng từ session (cần extension methods cho Session nếu chưa có)
        Cart cart = _httpContextAccessor.HttpContext!.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();
        // Truyền Model cho View của View Component
        return View(cart);
    }
}