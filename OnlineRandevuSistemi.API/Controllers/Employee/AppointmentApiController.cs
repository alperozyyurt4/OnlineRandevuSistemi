using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Api.ViewModels;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Enums;
using System.Security.Claims;

namespace OnlineRandevuSistemi.API.Controllers.Employee
{
    [ApiController]
    [Route("api/employee/appointments")]
    [Authorize(Roles = "Employee")]
    public class AppointmentApiController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<AppUser> _userManager;

        public AppointmentApiController(
            IAppointmentService appointmentService,
            IEmployeeService employeeService,
            UserManager<AppUser> userManager)
        {
            _appointmentService = appointmentService;
            _employeeService = employeeService;
            _userManager = userManager;
        }

        private async Task<int?> GetEmployeeIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var emp = await _employeeService.GetEmployeeByUserIdAsync(userId);
            return emp?.Id;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employeeId = await GetEmployeeIdAsync();
            if (employeeId == null) return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employeeId.Value);
            return Ok(appointments.OrderByDescending(a => a.AppointmentDate));
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming()
        {
            var employeeId = await GetEmployeeIdAsync();
            if (employeeId == null) return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employeeId.Value);

            var upcoming = appointments
                .Where(a => a.AppointmentDate >= DateTime.Now &&
                            (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending))
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            return Ok(upcoming);
        }

        [HttpGet("past")]
        public async Task<IActionResult> GetPast()
        {
            var employeeId = await GetEmployeeIdAsync();
            if (employeeId == null) return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employeeId.Value);

            var past = appointments
                .Where(a => a.AppointmentDate < DateTime.Now)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return Ok(past);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound();

            return Ok(appointment);
        }

        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] EmployeeAppointmentStatusUpdateDto model)
        {
            if (!Enum.IsDefined(typeof(AppointmentStatus), model.NewStatus))
                return BadRequest("Geçersiz durum.");

            try
            {
                await _appointmentService.UpdateAppointmentStatusAsync(model.Id, model.NewStatus);
                return Ok(new { message = "Randevu durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
public class EmployeeAppointmentStatusUpdateDto
{
    public int Id { get; set; }
    public AppointmentStatus NewStatus { get; set; }
}

