using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportsStoreWebApp.Models;

namespace SportsStoreWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly MyService _myService;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env, MyService myService)
    {
        _logger = logger;
        _env = env;
        _myService = myService;
    }

    // Hành động Index khi người dùng truy cập trang chủ (ví dụ: http://localhost:xxxx/)
    public IActionResult Index()
    {
        ViewBag.UserAgent = _myService.GetCurrentUserAgent();
        // Kiểm tra môi trường hiện tại bằng các phương thức mở rộng
        if (_env.IsDevelopment()) // Nếu ứng dụng đang chạy trong môi trường Development(Phát triển)
        {
            ViewBag.EnvironmentMessage = "Bạn đang ở môi trường Phát triển.";
        }
        else if (_env.IsStaging()) // Nếu ứng dụng đang chạy trong môi trường Staging(Kiểm thử trước khi triển khai)
        {
            ViewBag.EnvironmentMessage = "Bạn đang ở môi trường Staging.";
        }
        else if (_env.IsProduction()) // Nếu ứng dụng đang chạy trong môi trường Production(Sản xuất)
        {
            ViewBag.EnvironmentMessage = "Bạn đang ở môi trường Sản xuất.";
        }
        else // Nếu không khớp với bất kỳ môi trường chuẩn nào
        {
            ViewBag.EnvironmentMessage = $"Bạn đang ở môi trường: {_env.EnvironmentName}.";
        }
        // ViewBag là một cách để truyền dữ liệu từ Controller sang View.
        // Biến EnvironmentMessage sẽ được hiển thị trong View.
        return View(); // Trả về View tương ứng với hành động Index (Index.cshtml)
    }
    public IActionResult AboutUs()
    {
        _logger.LogInformation("Yêu cầu trang Giới thiệu.");
        // Có thể truyền một thông điệp đơn giản
        ViewBag.Message = "Đây là trang giới thiệu về chúng tôi.";
        return View(); // Trả về Views/Home/AboutUs.cshtml (cần tạo)
    }

    // Ví dụ trả về JsonResult
    public IActionResult GetServerTime()
    {
        _logger.LogInformation("Đã yêu cầu thời gian máy chủ.");
        return Json(new
        {
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Date = DateTime.Now.ToShortDateString()
        });
    }
    // Ví dụ chuyển hướng
    public IActionResult GoToProductList()
    {
        _logger.LogInformation("Đang chuyển hướng đến danh sách sản phẩm.");
        return RedirectToAction("List", "Product"); // Chuyển hướng đến ProductController.List
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Lấy thông tin lỗi
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (exceptionHandlerPathFeature?.Error != null)
        {
            // Sử dụng _logger instance để ghi log lỗi
            _logger.LogError(exceptionHandlerPathFeature.Error, "Một ngoại lệ chưa được xử lý đã xảy ra tại { Path}", exceptionHandlerPathFeature.Path);
        }
        else
        {
            // Ghi log nếu không có thông tin lỗi cụ thể từ exception handler
            _logger.LogWarning("Đã gọi hành động lỗi nhưng không tìm thấy tính năng ngoại lệ cụ thể nào.");
        }
        // Truyền RequestId để người dùng có thể báo cáo lỗi
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
    // Action để giả lập lỗi 500
    public IActionResult SimulateFatalError()
    {
        throw new InvalidOperationException("Đây là một ngoại lệ được cố ý đưa ra để kiểm tra cách xử lý lỗi!!");
    }
}


