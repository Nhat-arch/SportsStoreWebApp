using Microsoft.EntityFrameworkCore; // Để dùng ToListAsync, etc.
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Models;
namespace SportsStore.Infrastructure.Repositories
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private ApplicationDbContext _context;
        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public IQueryable<Category> Categories => _context.Categories; // Chỉ đơn giản trả về DbSet
                                                                  // Triển khai phương thức SaveCategory từ ICategoryRepository
    }
}