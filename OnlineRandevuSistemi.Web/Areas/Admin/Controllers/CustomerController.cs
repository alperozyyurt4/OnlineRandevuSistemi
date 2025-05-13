using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Web.ViewModels;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly UserManager<AppUser> _userManager;

        public CustomerController(ICustomerService customerService, UserManager<AppUser> userManager)
        {
            _customerService = customerService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string sortOrder)
        {
            var customers = await _customerService.GetAllCustomersAsync();

            string currentColumn = "";
            string currentDirection = "asc";

            customers = sortOrder switch
            {
                "name_desc" => customers.OrderByDescending(c => c.FirstName),
                "lastname_asc" => customers.OrderBy(c => c.LastName),
                "lastname_desc" => customers.OrderByDescending(c => c.LastName),
                "email_asc" => customers.OrderBy(c => c.Email),
                "email_desc" => customers.OrderByDescending(c => c.Email),
                _ => customers.OrderBy(c => c.FirstName)
            };

            // İkon kontrolü için ViewBag
            if (!string.IsNullOrEmpty(sortOrder))
            {
                var parts = sortOrder.Split('_');
                currentColumn = parts[0];
                currentDirection = parts.Length > 1 && parts[1] == "desc" ? "desc" : "asc";
            }

            ViewBag.CurrentSortColumn = currentColumn;
            ViewBag.CurrentSortDirection = currentDirection;

            ViewBag.NameSort = sortOrder == "name_desc" ? "name_asc" : "name_desc";
            ViewBag.LastNameSort = sortOrder == "lastname_desc" ? "lastname_asc" : "lastname_desc";
            ViewBag.EmailSort = sortOrder == "email_desc" ? "email_asc" : "email_desc";

            return View(customers);
        }



        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                ProfilePicture = model.ProfilePicture ?? "/images/default-profile.jpg"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            await _customerService.CreateCustomerAsync(new CustomerDto
            {
                UserId = user.Id,
                PreferredLanguage = "TR",
                Notes = "Kayıt müşteri"
            });

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound();

            var model = new CustomerEditViewModel
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
        public async Task<IActionResult> Edit(CustomerEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new CustomerDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth,
                Notes = model.Notes
            };

            await _customerService.UpdateCustomerAsync(dto);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return RedirectToAction("Index");
        }

    }
}