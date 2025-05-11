using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Enums;
using System.Security.Claims;

namespace OnlineRandevuSistemi.API.Controllers.Employee
{
    [ApiController]
    [Route("api/employee/dashboard")]
    [Authorize(Roles = "Employee")]
    public class DashboardApiController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<AppUser> _userManager;

        public DashboardApiController(
            IAppointmentService appointmentService,
            IEmployeeService employeeService,
            UserManager<AppUser> userManager)
        {
            _appointmentService = appointmentService;
            _employeeService = employeeService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
            if (employee == null)
                return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employee.Id);

            var result = new
            {
                TotalAppointments = appointments.Count(),
                UpcomingAppointments = appointments.Count(a =>
                    a.AppointmentDate >= DateTime.Now &&
                    (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed)),

                PastAppointments = appointments.Count(a => a.AppointmentDate < DateTime.Now)
            };

            return Ok(result);
        }
    }
}