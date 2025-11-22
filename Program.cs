using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SportsStore.Domain.Abstract;
using SportsStore.Infrastructure;
using SportsStore.Infrastructure.Repositories;
using SportsStoreWebApp.Configurations;
using Swashbuckle.AspNetCore.SwaggerGen;


var builder = WebApplication.CreateBuilder(args);

// Cấu hình JWT Settings từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Kiểm tra thời hạn token
        ValidateIssuerSigningKey = true, // Kiểm tra chữ ký token
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});
// Thêm Authorization (phải sau Authentication)
builder.Services.AddAuthorization(); // Đảm bảo AddAuthorization() được gọi

builder.Services.AddEndpointsApiExplorer(); // Cần thiết cho API Explorer
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SportsStore API",
        Version = "v1"
    });
    // Tùy chỉnh để hỗ trợ JWT Authentication trong Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    // Tích hợp XML comments để tài liệu hóa API
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});



builder.Services.AddDbContext<ApplicationDbContext>(options =>

options.UseSqlServer(builder.Configuration.GetConnectionString("SportsStoreConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Tùy chỉnh các yêu cầu về mật khẩu, lockout... (rất quan trọng cho bảo mật)
    options.SignIn.RequireConfirmedAccount = false; // Không yêu cầu xác nhận email/tài khoản
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
})
.AddRoles<IdentityRole>() // Thêm hỗ trợ Role (quan trọng cho phân quyền)
.AddEntityFrameworkStores<ApplicationDbContext>(); // Đăng ký Identity với DbContext đã tạo

// Đăng ký PagingSettings để có thể inject IOptions<PagingSettings> vào Controller
builder.Services.Configure<PagingSettings>(builder.Configuration.GetSection("PagingSettings"));

// Đăng ký dịch vụ Product Repository với vòng đời Scoped
// Mỗi yêu cầu HTTP sẽ nhận một thể hiện mới của FakeProductRepository

// builder.Services.AddScoped<IProductRepository, FakeProductRepository>();
builder.Services.AddScoped<IProductRepository, EFProductRepository>();// Thay thế bằng EFProductRepository
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SportsStore API V1");
    c.RoutePrefix = "swagger"; // Truy cập Swagger UI tại /swagger
});
app.UseDeveloperExceptionPage();

// if (app.Environment.IsDevelopment())
// {
// app.UseDeveloperExceptionPage(); // Trang lỗi chi tiết cho dev
// }
// else
// {
//     app.UseExceptionHandler("/Home/Error"); // Chuyển hướng đến trang lỗi tùy chỉnh
//     // app.UseHsts(); // Thường được dùng trong Production
// }

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapStaticAssets();

app.UseAuthentication(); // Phải đứng trước UseAuthorization

app.UseAuthorization();

app.MapRazorPages();

// Định tuyến cho các Controller trong Area Admin
app.MapControllerRoute(
 name: "Admin",
 pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"); //Ví dụ: / Admin / Product / Index

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

// // --- Bắt đầu phần thực hành C# cơ bản ---
// Console.WriteLine("--- Thực hành C# cơ bản ---");
// // Tạo danh sách sản phẩm mẫu
// List<SportsStore.Domain.Models.Product> sampleProducts = new
// List<SportsStore.Domain.Models.Product>
// {
// new SportsStore.Domain.Models.Product { ProductID = 1, Name = "Bóng đá World Cup", Description = "Bóng đá chính hãng", Price = 50.00m, Category = "Bóng đá" },
// new SportsStore.Domain.Models.Product { ProductID = 2, Name = "Áo đấu CLB A", Description = "Áo đấu cho người hâm mộ", Price = 75.50m, Category = "Quần áo" },
// new SportsStore.Domain.Models.Product { ProductID = 3, Name = "Vợt Tennis Pro", Description = "Vợt chuyên nghiệp", Price = 150.00m, Category = "Tennis" },
// new SportsStore.Domain.Models.Product { ProductID = 4, Name = "Giày chạy bộ ABC", Description = "Giày thể thao nhẹ", Price = 99.99m, Category = "Giày" },
// new SportsStore.Domain.Models.Product { ProductID = 5, Name = "Bóng rổ NBA",
// Description = "Bóng rổ tiêu chuẩn", Price = 45.00m, Category = "Bóng rổ" }
// };
// Console.WriteLine("\n--- LINQ: Lọc sản phẩm có giá trên 70 ---");
// var expensiveProducts = sampleProducts.Where(p => p.Price > 70.00m);
// foreach (var p in expensiveProducts)
// {
//     Console.WriteLine($"- {p.Name} ({p.Price:C})");
// }
// Console.WriteLine("\n--- LINQ: Lấy sản phẩm đầu tiên thuộc danh mục 'Bóng đá' ---");
// var firstFootballProduct = sampleProducts.FirstOrDefault(p => p.Category == "Bóng đá");
// if (firstFootballProduct != null)
// {
//     Console.WriteLine($"- {firstFootballProduct.Name}");
// }
// else
// {
//     Console.WriteLine("Không tìm thấy sản phẩm bóng đá.");
// }
// Console.WriteLine("\n--- Async/Await: Mô phỏng thao tác bất đồng bộ ---");
// async Task SimulateDataFetchAsync()
// {
//     Console.WriteLine("Đang bắt đầu lấy dữ liệu (mất 2 giây)...");
//     await Task.Delay(2000); // Mô phỏng thao tác tốn thời gian
//     Console.WriteLine("Đã lấy xong dữ liệu.");
// }
// // Gọi hàm bất đồng bộ
// await SimulateDataFetchAsync(); // Cần `await` ở đây vì hàm Main của .NET 6+ đã là async
// Console.WriteLine("--- Kết thúc thực hành C# cơ bản ---\n");
// // --- Kết thúc phần thực hành C# cơ bản ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Áp dụng tất cả các Migrations chưa áp dụng (nếu có)
        context.Database.Migrate();
        // Seed Identity Roles và User
        await AppIdentityDbContextSeed.SeedRolesAndUsers(services);
        // Seed dữ liệu sản phẩm/danh mục ban đầu (nếu chưa có trong Migration)
        // await SeedData.EnsurePopulated(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}
// ... các middleware khác ...

app.Run();
