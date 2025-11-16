using Microsoft.EntityFrameworkCore;
using SportsStore.Domain.Models;

namespace SportsStore.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; } = default!; // Ánh xạ Product Model tới bảng Products
        public DbSet<Category> Categories { get; set; } = default!; // Ánh xạ Category Model tới bảng Categories
                                                                    // Có thể thêm DbSet cho các Model khác như Cart, Order...
                                                                    // public DbSet<Order> Orders { get; set; } = default!;
                                                                    // OnModelCreating() sẽ được thảo luận dưới đây
                                                                    // SportsStore.Infrastructure/ApplicationDbContext.cs
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình cho bảng Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products"); // Đặt tên bảng rõ ràng
                entity.HasKey(e => e.ProductID); // Đặt khóa chính
                entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100); // Ràng buộc độ dài và không null
                entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)"); // Kiểu dữ liệu chính xác cho cột

                entity.Property(e => e.Description)
                .HasMaxLength(500); // Giới hạn độ dài mô tả
                                    // Nếu có CategoryId làm khóa ngoại
                                    // entity.HasOne(p => p.CategoryRef) // Mối quan hệ một-nhiều với Category
                                    // .WithMany(c => c.Products)
                                    // .HasForeignKey(p => p.CategoryId);
            });
            // Cấu hình cho bảng Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.CategoryID);
                entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            });
            // Seed initial data (dữ liệu ban đầu)
            modelBuilder.Entity<Category>().HasData(
            new Category { CategoryID = 1, Name = "Bóng đá" },
            new Category { CategoryID = 2, Name = "Cờ vua" },
            new Category { CategoryID = 3, Name = "Bóng chuyền" }
            );
            modelBuilder.Entity<Product>().HasData(
            new Product
            {
                ProductID = 1,
                Name = "Bóng đá World Cup",
                Description = "Bóng đá chất lượng cao.",
                Price = 25.00m,
                Category = "Bóng đá",
                ImageUrl = " / images / football.jpg",
                CategoryId = 1
            },

            new Product
            {
                ProductID = 2,
                Name = "Bộ cờ vua chuyên nghiệp",
                Description = "Bộ cờ vua bằng gỗ cao cấp.",
                Price = 75.00m,
                Category = "Cờ vua",
                ImageUrl = "/images/chess.jpg",
                CategoryId = 2
            },
                new Product
                {
                    ProductID = 3,
                    Name = "Bóng chuyền bãi biển",
                    Description = "Bóng chuyền dành cho bãi biển.",
                    Price = 15.00m,
                    Category = "Bóng chuyền",
                    ImageUrl = "/images/volleyball.jpg",
                    CategoryId = 3
                }

            );
        }
    }
}