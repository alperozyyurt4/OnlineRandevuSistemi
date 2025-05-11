// Areas/Customer/Controllers/CustomerController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Web.ViewModels;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly UserManager<AppUser> _userManager;

        public CustomerController(ICustomerService customerService, UserManager<AppUser> userManager)
        {
            _customerService = customerService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _customerService.GetCustomerByUserIdAsync(user.Id);

            var model = new CustomerProfileViewModel
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                DateOfBirth = customer.DateOfBirth,
                Notes = customer.Notes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CustomerProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            var dto = new CustomerDto
            {
                Id = model.Id,
                UserId = user.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth ?? default,
                Notes = model.Notes
            };

            await _customerService.UpdateCustomerAsync(dto);

            TempData["Success"] = "Profil başarıyla güncellendi.";
            return RedirectToAction("Profile");
        }
    }
}