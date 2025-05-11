using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Web.ViewModels;
using System;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmployeeService _employeeService;
        private readonly IServiceService _serviceService;

        public EmployeeController(UserManager<AppUser> userManager, IEmployeeService employeeService, IServiceService serviceService)
        {
            _userManager = userManager;
            _employeeService = employeeService;
            _serviceService = serviceService;
        }


        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return View(employees);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new EmployeeCreateViewModel
            {
                Services = (await _serviceService.GetAllServicesAsync())
                            .Select(s => new SelectListItem
                            {
                                Value = s.Id.ToString(),
                                Text = s.Name
                            }).ToList()
            };

            return View(model);
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
                model.Services = (await _serviceService.GetAllServicesAsync())
                                .Select(s => new SelectListItem
                                {
                                    Value = s.Id.ToString(),
                                    Text = s.Name
                                }).ToList();
                return View(model);
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
                ProfilePicture = model.ProfilePicture ?? "/images/default-profile.jpg"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);

                model.Services = (await _serviceService.GetAllServicesAsync())
                                .Select(s => new SelectListItem
                                {
                                    Value = s.Id.ToString(),
                                    Text = s.Name
                                }).ToList();
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Employee");

            await _employeeService.CreateEmployeeAsync(new Business.DTOs.EmployeeDto
            {
                UserId = user.Id,
                IsAvailable = true,
                Biography = model.Biography ?? "Henüz eklenmedi",
                HourlyDate = 100, // sabit örnek değer
                Position = "Genel", // sabit örnek değer
                ServiceIds = model.SelectedServiceIds // 💥 Hizmet ataması burada
            });

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var emp = await _employeeService.GetEmployeeByIdAsync(id);
            if (emp == null) return NotFound();

            var model = new EmployeeEditViewModel
            {
                Id = emp.Id,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                Email = emp.Email,
                PhoneNumber = emp.PhoneNumber,
                Address = emp.Address,
                DateOfBirth = emp.DateOfBirth,
                Biography = emp.Biography,
                ProfilePicture = emp.ProfilePicture,
                SelectedServiceIds = emp.ServiceIds,
                Services = (await _serviceService.GetAllServicesAsync())
                            .Select(s => new SelectListItem
                            {
                                Value = s.Id.ToString(),
                                Text = s.Name
                            }).ToList()
            };

            ViewBag.EmployeeId = id;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Services = (await _serviceService.GetAllServicesAsync())
                                 .Select(s => new SelectListItem
                                 {
                                     Value = s.Id.ToString(),
                                     Text = s.Name
                                 }).ToList();
                ViewBag.EmployeeId = id;
                return View(model);
            }

            var dto = new EmployeeDto
            {
                Id = id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                Biography = model.Biography,
                ProfilePicture = model.ProfilePicture ?? "/images/default-profile.jpg",
                Position = "Güncellendi",
                HourlyDate = 150,
                IsAvailable = true,
                ServiceIds = model.SelectedServiceIds
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