using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.Interfaces;
using Microsoft.AspNetCore.Identity;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Web.ViewModels;
using OnlineRandevuSistemi.Core.Enums;
using Microsoft.AspNetCore.Authorization;
namespace OnlineRandevuSistemi.Web.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class HomeController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(
            IAppointmentService appointmentService,
            IEmployeeService employeeService,
            UserManager<AppUser> userManager)
        {
            _appointmentService = appointmentService;
            _employeeService = employeeService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Id);
            var appointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employee.Id);

            var model = new EmployeeDashboardViewModel
            {
                TotalAppointments = appointments.Count(),
                UpcomingAppointments = appointments.Count(a => a.AppointmentDate >= DateTime.Now && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed)),
                PastAppointments = appointments.Count(a => a.AppointmentDate < DateTime.Now)
            };

            return View(model);
        }
    }
}