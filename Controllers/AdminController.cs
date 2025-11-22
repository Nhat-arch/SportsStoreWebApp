using System.Linq; // Cần cho IQueryable
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Models;
namespace SportsStoreWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private IProductRepository _repository;
        private ICategoryRepository _cate;
        public AdminController(IProductRepository repo, ICategoryRepository cate)
        {
            _repository = repo;
            _cate = cate;
        }
        public ViewResult Index() => View(_repository.Products); // Hiển thị tất cả sản phẩm trong Admin Panel
                                                                 // GET: Admin/Edit/5 hoặc Admin/Edit
        public async Task<IActionResult> Edit(int productId)
        {
            Product? product = await _repository.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
            if (product == null)
            {
                return NotFound();
            }
            // Nếu bạn có danh mục riêng, bạn có thể tải chúng vào ViewBag để dùng cho dropdown
            ViewBag.Categories = await _cate.Categories.ToListAsync();
            return View(product);
        }
        // POST: Admin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                await _repository.SaveProduct(product);
                TempData["message"] = $"{product.Name} đã được lưu thành công.";
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang Index của Admin
            }
            else
            {
                // Dữ liệu không hợp lệ, hiển thị lại form
                return View(product);
            }
        }
        // GET: Admin/Create
        public async Task<ViewResult> Create()
        {
            ViewBag.Categories = await _cate.Categories.ToListAsync();
            return View("Edit", new Product());
        }  // Tái sử dụng View Edit cho Create
        // POST: Admin/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int productId)
        {
            Product? deletedProduct = await _repository.DeleteProduct(productId);
            if (deletedProduct != null)
            {
                TempData["message"] = $"{deletedProduct.Name} đã được xóa.";
            }
            else
            {
                TempData["message"] = $"Không tìm thấy sản phẩm có ID: {productId} để xóa.";
                TempData["messageType"] = "danger"; // Báo lỗi
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


