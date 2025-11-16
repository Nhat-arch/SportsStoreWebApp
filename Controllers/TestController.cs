using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SportsStoreWebApp.Models;

namespace SportsStoreWebApp.Controllers
{
    public class TestController : Controller
    {
        // Action Method mặc định khi truy cập /Test
        public IActionResult Index()
        {
            ViewBag.Message = "Chào mừng bạn đến với Cửa hàng Thể thao! Đây là trang Test!";
            return View(); // Trả về View có tên Index
        }
        // Một Action Method khác: /Test/HelloWorld
        public IActionResult HelloWorld()
        {
            return Content("Xin chào từ Action HelloWorld của TestController!");
        }
        // Action nhận tham số: /Test/Welcome?name=DavidTeo
        public IActionResult Welcome(string name = "Khách")
        {
            return Content($"Chào mừng {name} đến với trang Test!");
        }
    }
}