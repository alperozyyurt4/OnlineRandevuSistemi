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

// Bağlantı dizesi
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection bulunamadı.");

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Yetkisiz yönlendirme işlemleri
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// Memory Cache (GetPopularServices gibi yapılar için)
builder.Services.AddMemoryCache();

// Redis bağlantısı (StackExchange.Redis ile)
/*
try
{
    var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection")
        ?? "localhost:6379,abortConnect=false";

    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
    builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
}
catch (Exception ex)
{
    Console.WriteLine("⚠ Redis'e bağlanılamadı. Cache devre dışı. => " + ex.Message);
}
*/

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile), typeof(WebMappingProfile));

// Repository & UnitOfWork
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Business Services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IEmployeeService, OnlineRandevuSistemi.Business.Services.EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmailService, DummyEmailService>();
builder.Services.AddScoped<ISmsService, DummySmsService>();

//Background Services
builder.Services.AddHostedService<AppointmentReminderService>();

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await SeedData.InitializeAsync(services);
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
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