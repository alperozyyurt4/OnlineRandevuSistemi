using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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

   
        
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}