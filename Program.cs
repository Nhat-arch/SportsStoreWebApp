using SportsStore.Domain.Abstract;
using SportsStoreWebApp.Concrete;
using SportsStoreWebApp.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký PagingSettings để có thể inject IOptions<PagingSettings> vào Controller
builder.Services.Configure<PagingSettings>(builder.Configuration.GetSection("PagingSettings"));

// Đăng ký dịch vụ Product Repository với vòng đời Scoped
// Mỗi yêu cầu HTTP sẽ nhận một thể hiện mới của FakeProductRepository
builder.Services.AddScoped<IProductRepository, FakeProductRepository>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MyService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian sống của session khi không hoạt động
    options.Cookie.HttpOnly = true; // Không cho JS truy cập cookie session
    options.Cookie.IsEssential = true; // Cookie session là cần thiết cho chức năng
});

var app = builder.Build();
// Đăng ký Middleware tùy chỉnh đầu tiên trong pipeline
app.UseMiddleware<SportsStoreWebApp.Middleware.RequestLoggerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Trang lỗi chi tiết cho dev
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Chuyển hướng đến trang lỗi tùy chỉnh
    // app.UseHsts(); // Thường được dùng trong Production
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

// 1. Tuyến đường cho phân trang CÓ DANH MỤC: Ví dụ: /Bong%20da/Page2
// - Bắt tham số {category} (chuỗi) và {productPage} (số nguyên)
app.MapControllerRoute(
    name: "category_page", // Tên route này để bạn có thể tham chiếu nếu cần
    pattern: "{category}/Page{productPage:int}", // Mẫu URL: {category} (tên danh mục) / Page{ productPage} (số trang)
    defaults: new { Controller = "Product", action = "List" } //Controller và Action mặc định
);

// 2. Tuyến đường cho phân trang KHÔNG CÓ DANH MỤC (chỉ trang): Ví dụ: / Page2
// - Chỉ bắt tham số {productPage} (số nguyên)
app.MapControllerRoute(
    name: "pagination",
    pattern: "Page{productPage:int}", // Mẫu URL: Page{productPage}
    defaults: new { Controller = "Product", action = "List" }
);

// 3. Tuyến đường cho LỌC THEO DANH MỤC (trang đầu tiên): Ví dụ: / Bong % 20da
// - Bắt tham số {category} (chuỗi)
app.MapControllerRoute(
    name: "category",
    pattern: "{category}", // Mẫu URL: {category} (tên danh mục)
    defaults: new
    {
        Controller = "Product",
        action = "List",
        productPage = 1
    } // Mặc định là trang 1
);

// 4. Tuyến đường MẶC ĐỊNH (tổng quát nhất): Ví dụ: /, /Product/List, / Product / Details / 1
// - Bắt tham số {controller}, {action}, và {id} (tùy chọn)
app.MapControllerRoute(
name: "default",
pattern: "{controller=Product}/{action=List}/{id?}"); // Đặt Product / List làm trang mặc định khi truy cập root URL

// *** Route cụ thể hơn: Ví dụ cho các URL có cấu trúc rõ ràng cho sản phẩm theo danh mục ***
app.MapControllerRoute(
    name: "product_by_category",
    pattern: "san-pham/danh-muc/{category}", // URL sẽ là /san-pham/danhmuc/bong-da
    defaults: new { controller = "Product", action = "ListByCategory" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// --- Bắt đầu phần thực hành C# cơ bản ---
Console.WriteLine("--- Thực hành C# cơ bản ---");
// Tạo danh sách sản phẩm mẫu
List<SportsStore.Domain.Models.Product> sampleProducts = new
List<SportsStore.Domain.Models.Product>
{
new SportsStore.Domain.Models.Product { ProductID = 1, Name = "Bóng đá World Cup", Description = "Bóng đá chính hãng", Price = 50.00m, Category = "Bóng đá" },
new SportsStore.Domain.Models.Product { ProductID = 2, Name = "Áo đấu CLB A", Description = "Áo đấu cho người hâm mộ", Price = 75.50m, Category = "Quần áo" },
new SportsStore.Domain.Models.Product { ProductID = 3, Name = "Vợt Tennis Pro", Description = "Vợt chuyên nghiệp", Price = 150.00m, Category = "Tennis" },
new SportsStore.Domain.Models.Product { ProductID = 4, Name = "Giày chạy bộ ABC", Description = "Giày thể thao nhẹ", Price = 99.99m, Category = "Giày" },
new SportsStore.Domain.Models.Product { ProductID = 5, Name = "Bóng rổ NBA",
Description = "Bóng rổ tiêu chuẩn", Price = 45.00m, Category = "Bóng rổ" }
};
Console.WriteLine("\n--- LINQ: Lọc sản phẩm có giá trên 70 ---");
var expensiveProducts = sampleProducts.Where(p => p.Price > 70.00m);
foreach (var p in expensiveProducts)
{
    Console.WriteLine($"- {p.Name} ({p.Price:C})");
}
Console.WriteLine("\n--- LINQ: Lấy sản phẩm đầu tiên thuộc danh mục 'Bóng đá' ---");
var firstFootballProduct = sampleProducts.FirstOrDefault(p => p.Category == "Bóng đá");
if (firstFootballProduct != null)
{
    Console.WriteLine($"- {firstFootballProduct.Name}");
}
else
{
    Console.WriteLine("Không tìm thấy sản phẩm bóng đá.");
}
Console.WriteLine("\n--- Async/Await: Mô phỏng thao tác bất đồng bộ ---");
async Task SimulateDataFetchAsync()
{
    Console.WriteLine("Đang bắt đầu lấy dữ liệu (mất 2 giây)...");
    await Task.Delay(2000); // Mô phỏng thao tác tốn thời gian
    Console.WriteLine("Đã lấy xong dữ liệu.");
}
// Gọi hàm bất đồng bộ
await SimulateDataFetchAsync(); // Cần `await` ở đây vì hàm Main của .NET 6+ đã là async
Console.WriteLine("--- Kết thúc thực hành C# cơ bản ---\n");
// --- Kết thúc phần thực hành C# cơ bản ---
app.Run();
