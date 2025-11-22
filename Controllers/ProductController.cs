// Controllers/ProductController.cs
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Models;
using SportsStoreWebApp.Configurations;
// Áp dụng một route tiền tố cho toàn bộ Controller nếu muốn dùng Attribute Routing mạnh mẽ
// [Route("san-pham")] // Ví dụ: mọi action sẽ bắt đầu bằng /cua-hang/

namespace SportsStoreWebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductController> _logger; // Khai báo một logger

        private readonly PagingSettings _pagingSettings;
        public ProductController(IProductRepository repository, IConfiguration configuration, ILogger<ProductController> logger, IOptions<PagingSettings> pagingSettings)
        {
            _repository = repository;
            _configuration = configuration;
            _logger = logger;
            _pagingSettings = pagingSettings.Value; // Lấy đối tượng PagingSettings từ IOptions
            logger.LogInformation("ProductController đã được tạo."); // Log khi Controller được tạo
        }
        // Hành động GET: /Product/Create
        // Hiển thị biểu mẫu để người dùng nhập thông tin sản phẩm
        public IActionResult Create()
        {
            return View(); // Trả về view Create.cshtml
        }
        // Hành động POST: /Product/Create
        // Xử lý dữ liệu khi người dùng gửi biểu mẫu tạo sản phẩm
        [HttpPost] // Chỉ xử lý các yêu cầu POST
        [ValidateAntiForgeryToken] // Giúp bảo vệ khỏi tấn công giả mạo yêucầu chéo trang(CSRF)
        public IActionResult Create(Product newProduct)
        {
            // Log thông tin về việc cố gắng tạo sản phẩm (mức Information)
            _logger.LogInformation("Đang cố gắng tạo sản phẩm mới: { ProductName}", newProduct.Name);
            // Kiểm tra xem dữ liệu model có hợp lệ không (dựa trên các Data Annotations trong Product.cs)
            if (ModelState.IsValid)
            {
                // Logic để lưu sản phẩm vào cơ sở dữ liệu hoặc một danh sách tạm thời
                // Trong ví dụ này, chúng ta chỉ mô phỏng việc gán một IDngẫu nhiên
                newProduct.ProductID = new Random().Next(1000, 9999);
                // Log thông tin thành công (mức Information)
                _logger.LogInformation("Sản phẩm {ProductName} (ID: { ProductID}) đã được tạo thành công.", newProduct.Name, newProduct.ProductID);
                // Lưu thông báo thành công vào TempData để hiển thị ở trang khác(List)
                TempData["SuccessMessage"] = $"Sản phẩm '{newProduct.Name}' đã được tạo thành công với ID: {newProduct.ProductID}";
                // Chuyển hướng người dùng đến hành động "List" sau khi tạo thành công
                return RedirectToAction("List");
            }
            // Nếu ModelState không hợp lệ (validation thất bại)
            // Log cảnh báo (mức Warning)
            _logger.LogWarning("Tạo sản phẩm thất bại do trạng thái model không hợp lệ cho sản phẩm: { ProductName}", newProduct.Name);
            // Log chi tiết từng lỗi validation (mức Error)
            foreach (var modelStateEntry in ModelState.Values)
            {
                foreach (var error in modelStateEntry.Errors)
                {
                    _logger.LogError("Lỗi trạng thái Model: {ErrorMessage}",
                   error.ErrorMessage);
                }
            }
            // Trả về lại view Create với dữ liệu đã nhập (để người dùng sửa lỗi)
            return View(newProduct);
        }
        // Hành động GET: /Product/ErrorExample
        // Minh họa việc ghi log lỗi khi có ngoại lệ (exception) xảy ra
        public IActionResult ErrorExample()
        {
            try
            {
                // Cố tình ném ra một ngoại lệ để minh họa
                throw new Exception("Đây là một lỗi mô phỏng cho mục đích ghi log.");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi (mức Error) bao gồm cả thông tin chi tiết về ngoại lệ
                _logger.LogError(ex, "Một ngoại lệ chưa được xử lý đã xảy ra trong hành động ErrorExample.");
            }
            return View(); // Trả về view ErrorExample.cshtml
        }
        // Ví dụ 1: Convention-based Routing (không có [Route] attribute ở đây)
        // Sẽ khớp với /Product/List hoặc /Product (nếu List là action mặc định của ProductController)
        public IActionResult List(string? category = null, int productPage = 1) // Tham số category để lọc, có thể null
        {
            _logger.LogInformation("Yêu cầu danh sách sản phẩm. Danh mục: { Category}, Trang: { Page} ", category ?? "Tất cả", productPage);
            // Lấy số sản phẩm trên mỗi trang từ cấu hình PagingSettings
            int itemsPerPage = _pagingSettings.ItemsPerPage;
            //int maxPagesToShow = _pagingSettings.MaxPagesToShow; // Có thể dùng sau nếu muốn giới hạn số nút trang hiển thị
            // Lọc sản phẩm theo danh mục (nếu category không null hoặc rỗng)
            // Sau đó sắp xếp và thực hiện phân trang (Skip/Take)
            var productsQuery = _repository.Products.Where(p => category == null || p.CategoryRef.Name == category);
            var products = productsQuery
            .OrderBy(p => p.ProductID) //Quan trọng: Sắp xếp trước khi Skip / Take để đảm bảo phân trang đúng thứ tự
            .Skip((productPage - 1) * itemsPerPage) // Bỏ qua các sản phẩm của các trang trước đó
            .Take(itemsPerPage) // Lấy số sản phẩm bằng ItemsPerPage cho trang hiện tại
            .ToList(); // Chuyển kết quả sang List để truyền cho View
                       // Chuẩn bị dữ liệu cần thiết cho View thông qua ViewBag
            ViewBag.Categories = _repository.Products
            .Select(p => p.CategoryRef.Name)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
            ViewBag.CurrentCategory = category ?? "Tất cả sản phẩm"; //Danh mục hiện tại
            ViewBag.CurrentPage = productPage; // Trang hiện tại
            ViewBag.TotalItems = productsQuery.Count(); // Tổng số sản phẩm SAU KHI lọc, nhưng TRƯỚC KHI phân trang
            ViewBag.ItemsPerPage = itemsPerPage; // Số sản phẩm trên mỗi trang
                                                 // Ghi log thông tin về số lượng sản phẩm được trả về
                                                 //_logger.LogInformation("Trả về {ProductCount} sản phẩm cho trang { Page}. Tổng số sản phẩm: { TotalItems} ", products.Count, productPage, ViewBag.TotalItems);
                                                 // Trả về View với danh sách sản phẩm của trang hiện tại làm Model
            return View(products);
        }
        // Ví dụ 2: Action này sẽ được gọi bởi route "product_by_category" trong Program.cs
        // public IActionResult ListByCategory(string category) { /* Logic tương tự List(category)*/ }
        // (Chúng ta có thể gộp logic vào một Action `List` duy nhất như trên)
        // Ví dụ 3: Attribute Routing cho chi tiết sản phẩm
        // Sẽ khớp với /product/chi-tiet/{id}
        // [Route("product/chi-tiet/{id:int}")] // Nếu không có tiền tố Controller Route
        [Route("chi-tiet/{id:int}")] // Nếu có [Route("product")] ở cấp Controller
                                     // Hành động Details để hiển thị chi tiết một sản phẩm
        public IActionResult Details(int id)
        {
            var product = _repository.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                // Ghi log cảnh báo nếu không tìm thấy sản phẩm
                _logger.LogWarning("Không tìm thấy sản phẩm với ID: { ProductID}.", id);
                return NotFound(); // Trả về lỗi 404 Not Found
            }
            // Ghi log thông tin khi hiển thị chi tiết sản phẩm
            _logger.LogInformation("Hiển thị chi tiết sản phẩm ID: { ProductID} ", id);
            return View(product);
        }
        // Tạo một Action để kiểm tra ghi log lỗi
        public IActionResult SimulateError()
        {
            try
            {
                _logger.LogWarning("Mô phỏng lỗi để kiểm tra nhật ký...");
                throw new InvalidOperationException("Đây là lỗi kiểm tra từ SimulateError action!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi không mong muốn trong quá trình mô phỏng lỗi!");
            }
            return Content("Kiểm tra đầu ra console/debug của bạn để tìm nhật ký!");
        }
        public IActionResult FilterProducts(ProductFilter filter) // Model Binding cho ProductFilter
        {
            _logger.LogInformation("Lọc sản phẩn theo Category: {Category}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice }, InStockOnly: { InStock} ", filter.Category, filter.MinPrice, filter.MaxPrice, filter.InStockOnly); // Logic lọc sản phẩm dựa trên filter
            var filteredProducts = _repository.Products;
            if (!string.IsNullOrEmpty(filter.Category))
            {
                filteredProducts = filteredProducts.Where(p => p.CategoryRef.Name == filter.Category);
            }
            if (filter.MinPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price >= filter.MinPrice.Value);
            }
            if (filter.MaxPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price <= filter.MaxPrice.Value);
            }
            // Nếu InStockOnly = true, thì lọc thêm điều kiện này
            // if (filter.InStockOnly) { filteredProducts = filteredProducts.Where(p => p.IsInStock()); }
            return View("List", filteredProducts.ToList()); // Tái sử dụng ViewList
        }
    }
}

