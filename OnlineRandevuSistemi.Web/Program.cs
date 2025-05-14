using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Business.Mapping;
using OnlineRandevuSistemi.Business.Services;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Interfaces;
using OnlineRandevuSistemi.DataAccess;
using OnlineRandevuSistemi.DataAccess.Context;
using OnlineRandevuSistemi.DataAccess.Repositories;
using AutoMapper;
using StackExchange.Redis;
using OnlineRandevuSistemi.Web.Areas.Admin.Controllers;
using OnlineRandevuSistemi.Web.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Ortama göre appsettings yükleme
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); // Docker'dan gelen env'leri de al

// Db bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection bulunamadı.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// MemoryCache
builder.Services.AddMemoryCache();

// Redis
try
{
    var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection")
        ?? "localhost:6379,abortConnect=false";

    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
    builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
}
catch (Exception ex)
{
    Console.WriteLine("⚠ Redis bağlantı hatası: " + ex.Message);
}

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile), typeof(WebMappingProfile));

// Repository ve Service katmanları
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IEmployeeService, OnlineRandevuSistemi.Business.Services.EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmailService, DummyEmailService>();
builder.Services.AddScoped<ISmsService, DummySmsService>();

// Background Service
builder.Services.AddHostedService<AppointmentReminderService>();

// MVC ve Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Eğer ortam Docker veya Production ise port 80 dinlenmeli
if (builder.Environment.IsProduction() || builder.Environment.EnvironmentName == "Docker")
{
    app.Urls.Add("http://*:80");
}

// İlk başlatmada migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    using var scope = app.Services.CreateScope();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
