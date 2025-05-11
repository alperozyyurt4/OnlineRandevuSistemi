using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;

namespace OnlineRandevuSistemi.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/employees")]
    [Authorize(Roles = "Admin")]
    public class EmployeeApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmployeeService _employeeService;
        private readonly IServiceService _serviceService;

        public EmployeeApiController(UserManager<AppUser> userManager, IEmployeeService employeeService, IServiceService serviceService)
        {
            _userManager = userManager;
            _employeeService = employeeService;
            _serviceService = serviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var emp = await _employeeService.GetEmployeeByIdAsync(id);
            return emp == null ? NotFound() : Ok(emp);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeCreateDto model)
        {
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
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Employee");

            var dto = new EmployeeDto
            {
                UserId = user.Id,
                Biography = model.Biography ?? "Henüz eklenmedi",
                Position = model.Position ?? "Genel",
                HourlyDate = model.HourlyDate ?? 100,
                IsAvailable = true,
                ServiceIds = model.ServiceIds
            };

            await _employeeService.CreateEmployeeAsync(dto);
            return Ok(new { message = "Çalışan oluşturuldu." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateDto model)
        {
            if (id != model.Id)
                return BadRequest("ID uyuşmuyor.");

            var dto = new EmployeeDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                Biography = model.Biography,
                ProfilePicture = model.ProfilePicture ?? "/images/default-profile.jpg",
                Position = model.Position ?? "Güncellendi",
                HourlyDate = model.HourlyDate ?? 150,
                IsAvailable = true,
                ServiceIds = model.ServiceIds
            };

            await _employeeService.UpdateEmployeeAsync(dto);
            return Ok(new { message = "Çalışan güncellendi." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return Ok(new { message = "Çalışan silindi." });
        }
    }

    public class EmployeeCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Biography { get; set; }
        public string? Position { get; set; }
        public int? HourlyDate { get; set; }
        public List<int> ServiceIds { get; set; } = new();
    }

    public class EmployeeUpdateDto : EmployeeCreateDto
    {
        public int Id { get; set; }
    }
}