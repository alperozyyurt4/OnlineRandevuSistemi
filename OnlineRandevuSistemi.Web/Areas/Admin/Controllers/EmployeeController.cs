using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Web.ViewModels;
using System;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(UserManager<AppUser> userManager, IEmployeeService employeeService)
        {
            _userManager = userManager;
            _employeeService = employeeService;
        }


        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return View(employees);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
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
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                ProfilePicture = model.ProfilePicture ??= "/images/default-profile.jpg"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Employee");

            await _employeeService.CreateEmployeeAsync(new Business.DTOs.EmployeeDto
            {
                UserId = user.Id,
                IsAvailable = true,
                Biography = "Yeni çalışan",
                HourlyDate = 100,
                Position = "Genel"
            });

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var emp = await _employeeService.GetEmployeeByIdAsync(id);
            if (emp == null) return NotFound();

            var model = new EmployeeCreateViewModel
            {
                UserId = emp.UserId,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                Email = emp.Email,
                PhoneNumber = emp.PhoneNumber,
                Address = emp.Address,
                DateOfBirth = emp.DateOfBirth,
                Biography = emp.Biography,
                ProfilePicture = emp.ProfilePicture
            };

            ViewBag.EmployeeId = id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EmployeeId = id;
                return View(model);
            }

            var dto = new EmployeeDto
            {
                Id = id,
                UserId = model.UserId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                Biography = model.Biography,
                ProfilePicture = model.ProfilePicture  ??= "/images/default-profile.jpg",
                Position = "Güncellendi", // örnek
                HourlyDate = 150, // örnek
                IsAvailable = true
            };

            await _employeeService.UpdateEmployeeAsync(dto);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return RedirectToAction("Index");
        }

    }
}