using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.DataAccess.Context;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.DataAccess
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                Console.WriteLine("SeedData başlatıldı.");  // TRY THIS
                var context = services.GetRequiredService<ApplicationDbContext>();
                Console.WriteLine("DbContext çekildi.");

                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await context.Database.MigrateAsync();
                await SeedRolesAsync(roleManager);
                await SeedUsersAsync(userManager);
                await SeedServicesAsync(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<SeedData>>();
                logger.LogError(ex, "Veritabanı başlangıç verileri yüklenirken hata oluştu.");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "Employee", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var admin = new AppUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    PhoneNumber = "5551234567",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Address = "Merkez Mah. No:1 İstanbul",
                    ProfilePicture = "/images/default-profile.jpg"

                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        private static async Task SeedServicesAsync(ApplicationDbContext context)
        {
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service
                    {
                        Name = "Saç Kesimi",
                        Description = "Profesyonel saç kesimi hizmeti",
                        Price = 150,
                        DurationMinutes = 30,
                        ImageUrl = "/images/haircut.jpg"
                    },
                    new Service
                    {
                        Name = "Sakal Tıraşı",
                        Description = "Bakımlı ve düzgün sakal tıraşı",
                        Price = 100,
                        DurationMinutes = 20,
                        ImageUrl = "/images/beard.jpg"
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
