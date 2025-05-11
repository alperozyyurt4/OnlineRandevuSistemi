using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Api.ViewModels;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.API.Controllers.Employee
{
    [ApiController]
    [Route("api/employee/profile")]
    [Authorize(Roles = "Employee")]
    public class ProfileApiController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<AppUser> _userManager;

        public ProfileApiController(IEmployeeService employeeService, UserManager<AppUser> userManager)
        {
            _employeeService = employeeService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
            if (employee == null) return NotFound();

            var profile = new
            {
                employee.Id,
                employee.UserId,
                employee.FirstName,
                employee.LastName,
                employee.Email,
                employee.PhoneNumber,
                employee.Address,
                employee.DateOfBirth,
                employee.Position,
                employee.Biography,
                employee.HourlyDate
            };

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] EmployeeProfileUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
            return Ok(new { message = "Profil başarıyla güncellendi." });
        }
    }
    
    public class EmployeeProfileUpdateDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Position { get; set; }
        public string Biography { get; set; }
        public decimal HourlyDate { get; set; }
    }
}