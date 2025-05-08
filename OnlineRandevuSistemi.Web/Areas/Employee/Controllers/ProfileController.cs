using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Web.ViewModels;
using System;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class ProfileController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(IEmployeeService employeeService, UserManager<AppUser> userManager)
        {
            _employeeService = employeeService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Id);

            var model = new EmployeeProfileEditViewModel
            {
                Id = employee.Id,
                UserId = employee.UserId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Address = employee.Address,
                DateOfBirth = employee.DateOfBirth,
                Position = employee.Position,
                Biography = employee.Biography,
                HourlyDate = employee.HourlyDate
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new EmployeeDto
            {
                Id = model.Id,
                UserId = model.UserId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth,
                Position = model.Position,
                Biography = model.Biography,
                HourlyDate = model.HourlyDate
            };

            await _employeeService.UpdateEmployeeAsync(dto);
            return RedirectToAction("Index", "Home");
        }
    }
}