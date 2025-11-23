using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Models;
namespace SportsStore.WebUI.Areas.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Area("Admin")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Chỉ Admin với JWT
    public class ProductsApiController : ControllerBase // Kế thừa từ ControllerBase
    {
        private readonly IProductRepository _repository;
        public ProductsApiController(IProductRepository repository)
        {
            _repository = repository;
        }
        // GET: api/admin/ProductsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return Ok(await _repository.Products.ToListAsync()); // Trả về 200 OK và danh sách sản phẩm
        }
        // GET: api/admin/ProductsApi/{id}
        [HttpGet("{id}")] // Tham số id từ route
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _repository.Products.FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound(); // Trả về 404 Not Found
            }
            return Ok(product); // Trả về 200 OK và sản phẩm
        }
        // POST: api/admin/ProductsApi
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromBody] Product
        product) // Dữ liệu từ request body
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về 400 Bad Request nếu validation lỗi
            }
            await _repository.SaveProduct(product); // ProductID = 0 sẽ tạo mới
            return CreatedAtAction(nameof(GetProduct), new
            {
                id =
           product.ProductID
            }, product); // Trả về 201 Created và URI của tài nguyên mới
        }
        // PUT: api/admin/ProductsApi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromBody] Product product)
        {
            if (id != product.ProductID)
            {
                return BadRequest(); // Trả về 400 Bad Request
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _repository.SaveProduct(product); // ProductID != 0 sẽ cập nhật
            }
            catch (DbUpdateConcurrencyException) // Xử lý xung đột (nếu có Concurrency Token)
            {
                if (!await ProductExists(id))
                {
                    return NotFound(); // Trả về 404 Not Found
                }
                else
                {
                    throw;
                }
            }
            return NoContent(); // Trả về 204 No Content (thành công nhưng khôngcó nội dung trả về)
        }
        // DELETE: api/admin/ProductsApi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _repository.DeleteProduct(id); // Phương thức DeleteProduct trả về Product đã xóa hoặc null
            if (product == null)
            {
                return NotFound(); // Trả về 404 Not Found
            }
            return NoContent(); // Trả về 204 No Content
        }
        private async Task<bool> ProductExists(int id)
        {
            return await _repository.Products.AnyAsync(e => e.ProductID == id);
        }
    }
}