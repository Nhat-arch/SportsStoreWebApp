using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Domain.Abstract;
public class NavigationMenuViewComponent : ViewComponent
{
    private IProductRepository _productRepository;
    public NavigationMenuViewComponent(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public IViewComponentResult Invoke()
    {
        // Lấy category hiện tại từ Route Data (nếu có) để highlight
        ViewBag.SelectedCategory = RouteData?.Values["category"];
        // Lấy danh sách các danh mục duy nhất từ sản phẩm
        var categories = _productRepository.Products
        .Select(x => x.Category)
        .Distinct()
        .OrderBy(x => x);
        return View(categories);
    }
}