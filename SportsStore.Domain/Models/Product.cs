// SportsStore.Domain/Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SportsStore.Domain.Models
{
    [Table("Products")] // Ánh xạ lớp Product tới bảng "Products"
    public class Product
    {
        [Key]
        [Display(Name = "Mã SP")]
        public int ProductID { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên sản phẩm phải từ 3 đến 100 ký tự.")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập mô tả.")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập giá.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải là số dương.")]
        [Column(TypeName = "decimal(18, 2)")] // Kiểu dữ liệu trong DB
        [Display(Name = "Giá")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Vui lòng chỉ định danh mục.")]
        [Display(Name = "Danh mục")]
        public string Category { get; set; } = string.Empty;
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ.")]
        [Display(Name = "URL hình ảnh")]
        public string? ImageUrl { get; set; } // Có thể null
                                              // Thêm một thuộc tính CategoryId để liên kết với Category Model (sẽ tạo sau)
                                              // public int CategoryId { get; set; }
                                              // public virtual Category? CategoryRef { get; set; } //Navigation property(nếu có EF Core)
    }
}