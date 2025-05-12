using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Business.Mapping;
using OnlineRandevuSistemi.Business.Services;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Interfaces;
using OnlineRandevuSistemi.DataAccess.Context;
using OnlineRandevuSistemi.DataAccess.Repositories;
using System.Text;
using AutoMapper;
using StackExchange.Redis; // ✅ Redis için eklendi

var builder = WebApplication.CreateBuilder(args);

// ?? Bağlantı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ?? Identity yapılandırması
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ?? JWT Ayarları
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

// ✅ Redis Bağlantısı
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

// ?? Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Online Randevu Sistemi API",
        Version = "v1"
    });

    // Swagger'da "Authorize" butonu için
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Bearer {token} formatında JWT girin.",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ?? Diğer servisler
builder.Services.AddMemoryCache(); // ServiceService içinde GetPopular gibi yerler için hâlâ aktif olabilir
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IEmployeeService, OnlineRandevuSistemi.Business.Services.EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddControllers();

var app = builder.Build();

// ?? Middleware
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Randevu Sistemi API v1");
});

app.UseAuthentication(); // JWT çalışsın diye
app.UseAuthorization();

app.MapControllers();
app.Run();