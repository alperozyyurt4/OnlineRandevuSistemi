using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineRandevuSistemi.Core.Enums;
using OnlineRandevuSistemi.Web.ViewModels;

namespace OnlineRandevuSistemi.Web.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<AppUser> _userManager;

        public AppointmentController(
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
            var currentUser = await _userManager.GetUserAsync(User);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(currentUser.Id);
            var appointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employee.Id);

            appointments = appointments.OrderByDescending(a => a.AppointmentDate).ToList();

            return View(appointments);
        }
        public async Task<IActionResult> Upcoming()
        {

            var today = DateTime.Today;
            var user = await _userManager.GetUserAsync(User);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Id);
            var allAppointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employee.Id);

            var upcoming = allAppointments
                .Where(a => a.AppointmentDate >= DateTime.Now &&
                            (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending))
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            return View("Upcoming", upcoming);
        }

        public async Task<IActionResult> Past()
        {
            var user = await _userManager.GetUserAsync(User);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Id);
            var allAppointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employee.Id);

            var past = allAppointments
                .Where(a => a.AppointmentDate < DateTime.Now)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View("Past", past);
        }

        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound();

            return View(appointment);
        }
        public async Task<IActionResult> EditStatus(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound();

            var model = new EmployeeAppointmentStatusViewModel
            {
                Id = appointment.Id,
                CurrentStatus = appointment.Status
            };

            ViewBag.StatusOptions = Enum.GetValues(typeof(AppointmentStatus))
                .Cast<AppointmentStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = ((int)s).ToString(),
                    Selected = s == appointment.Status
                }).ToList();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditStatus(EmployeeAppointmentStatusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StatusOptions = GetStatusOptions(model.CurrentStatus);
                return View(model);
            }

            try
            {
                await _appointmentService.UpdateAppointmentStatusAsync(model.Id, model.NewStatus);
                TempData["Success"] = "Durum başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.StatusOptions = GetStatusOptions(model.CurrentStatus);
                return View(model);
            }
        }

        // ViewBag için yeniden kullanılabilir metot
        private List<SelectListItem> GetStatusOptions(AppointmentStatus currentStatus)
        {
            return Enum.GetValues(typeof(AppointmentStatus))
                .Cast<AppointmentStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = ((int)s).ToString(),
                    Selected = s == currentStatus
                }).ToList();
        }
    }
}