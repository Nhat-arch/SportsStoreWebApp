using Microsoft.EntityFrameworkCore; // Để dùng ToListAsync, etc.
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Models;
namespace SportsStore.Infrastructure.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private ApplicationDbContext _context;
        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public IQueryable<Product> Products => _context.Products; // Chỉ đơn giản trả về DbSet
                                                                  // Triển khai phương thức SaveProduct từ IProductRepository
        public async Task SaveProduct(Product product)
        {
            if (product.ProductID == 0) // Đây là sản phẩm mới (Create)
            {
                _context.Products.Add(product);
            }
            else // Đây là sản phẩm đã có (Update, sẽ học sau)
            {
                // Logic update sẽ được triển khai trong tuần sau
                // _context.Entry(product).State = EntityState.Modified;
                var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == product.ProductID);
                if (existingProduct != null)
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Category = product.Category;
                    existingProduct.ImageUrl = product.ImageUrl;
                }
            }
            await _context.SaveChangesAsync(); // Lưu thay đổi vào DB
        }
        // Triển khai phương thức DeleteProduct (sẽ học sau)
        public async Task<Product?> DeleteProduct(int productId)
        {
            Product? dbEntry = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
            if (dbEntry != null)
            {
                _context.Products.Remove(dbEntry);
                await _context.SaveChangesAsync();
            }
            return dbEntry;
        }
    }
}