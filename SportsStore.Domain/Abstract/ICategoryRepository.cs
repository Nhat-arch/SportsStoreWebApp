using SportsStore.Domain.Models;

namespace SportsStore.Domain.Abstract
{
    public interface ICategoryRepository
    {
        // Thuộc tính để lấy tất cả sản phẩm
        IQueryable<Category> Categories { get; }
    }
}