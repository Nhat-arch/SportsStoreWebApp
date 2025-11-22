// SportsStore.Domain/Models/Customer.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SportsStore.Domain.Models
{
    [Table("Customers")] // Ánh xạ lớp Customer tới bảng "Customers"
    public class Customer
    {
        [Key]
        [Display(Name = "Mã Khách hàng")]
        public int CustomerID { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên khách hàng phải từ 3 đến 50 ký tự.")]
        [Display(Name = "Tên khách hàng")]
        public string Name { get; set; } = string.Empty;
    }
}