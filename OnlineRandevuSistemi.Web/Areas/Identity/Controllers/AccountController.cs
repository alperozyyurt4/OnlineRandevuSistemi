using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Web.ViewModels;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICustomerService _customerService;    

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, ICustomerService customerService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _customerService = customerService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", "Email boş olamaz.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                else if (await _userManager.IsInRoleAsync(user, "Customer"))
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                else if (await _userManager.IsInRoleAsync(user, "Employee"))
                    return RedirectToAction("Index", "Home", new { area = "Employee" });
            }

            ModelState.AddModelError("", "Geçersiz giriş.");
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    foreach (var e in error.Value.Errors)
                    {
                        Console.WriteLine($"Hata: {error.Key} - {e.ErrorMessage}");
                    }
                }
            }

                var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber, // Kullanıcı girmediyse boş geç
                Address = model.Address,
                DateOfBirth = DateTime.Now,
                ProfilePicture = "/images/default-profile.jpg"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            // Müşteri kaydı oluşturuluyor
            await _customerService.CreateCustomerAsync(new CustomerDto
            {
                UserId = user.Id,
                PreferredLanguage = "TR",
                Notes = "Kayıtlı müşteri"
            });

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home", new {area = "Customer"});
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }
}