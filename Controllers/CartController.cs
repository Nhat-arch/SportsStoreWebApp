// Controllers/CartController.cs
using Microsoft.AspNetCore.Mvc;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Models;
using Microsoft.Extensions.Logging;
using SportsStoreWebApp.Extensions; // Để dùng SessionExtensions
public class CartController : Controller
{
    private readonly IProductRepository _repository;
    private readonly ILogger<CartController> _logger; // Để ghi log
    public CartController(IProductRepository repo, ILogger<CartController> logger)
    {
        _repository = repo;
        _logger = logger;
    }
    // Action để thêm sản phẩm vào giỏ hàng
    public IActionResult AddToCart(int productId, string returnUrl)
    {
        Product? product = _repository.Products.FirstOrDefault(p => p.ProductID == productId);
        if (product != null)
        {
            // Lấy giỏ hàng từ Session hoặc tạo mới
            Cart cart = HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();
            cart.AddItem(product, 1); // Thêm 1 sản phẩm
            HttpContext.Session.SetObjectAsJson("Cart", cart); // Lưu lại giỏ hàng vào Session
            _logger.LogInformation("Added product {ProductName} (ID: { ProductID}) to cart. Total items in cart: { TotalItems}", product.Name, product.ProductID, cart.Lines.Sum(i => i.Quantity));
            //Lưu thông báo vào TempData
            TempData["message"] = $"{product.Name} đã được thêm vào giỏ hàng!";
            TempData["messageType"] = "success"; // Có thể dùng để tùy chỉnh CSS
        }
        else
        {
            _logger.LogWarning("Attempted to add non-existent product with ID { ProductID} to cart.", productId);
            TempData["message"] = $"Sản phẩm không tồn tại (ID: {productId}).";
            TempData["messageType"] = "danger";
        }
        return Redirect(returnUrl ?? "/"); // Chuyển hướng về trang trước
    }
    // Action để xem giỏ hàng
    public IActionResult Index(string returnUrl)
    {
        Cart cart = HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();
        ViewBag.ReturnUrl = returnUrl;
        return View(cart); // Truyền đối tượng Cart sang View
    }
    // Action để xóa sản phẩm khỏi giỏ hàng
    public IActionResult RemoveFromCart(int productId, string returnUrl)
    {
        Product? product = _repository.Products.FirstOrDefault(p => p.ProductID == productId);
        if (product != null)
        {
            Cart cart =
           HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();
            cart.RemoveItem(product);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            _logger.LogInformation("Removed product {ProductName} (ID: { ProductID}) from cart.", product.Name, product.ProductID);
        }
        return RedirectToAction("Index", new { returnUrl });
    }
}