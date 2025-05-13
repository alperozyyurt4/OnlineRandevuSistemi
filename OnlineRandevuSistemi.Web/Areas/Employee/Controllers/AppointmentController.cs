using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineRandevuSistemi.Core.Enums;
using OnlineRandevuSistemi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace OnlineRandevuSistemi.Web.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]

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
        public async Task<IActionResult> Index(string sortOrder)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(currentUser.Id);
            var appointments = (await _appointmentService.GetAppointmentsByEmployeeIdAsync(employee.Id))
                .Where(a => a.AppointmentDate.Date == DateTime.Today);

            var sortParts = (sortOrder ?? "").Split('_');
            var col = sortParts.ElementAtOrDefault(0) ?? "";
            var dir = sortParts.ElementAtOrDefault(1) ?? "asc";

            appointments = (col, dir) switch
            {
                ("customer", "asc") => appointments.OrderBy(a => a.CustomerName),
                ("customer", "desc") => appointments.OrderByDescending(a => a.CustomerName),
                ("service", "asc") => appointments.OrderBy(a => a.ServiceName),
                ("service", "desc") => appointments.OrderByDescending(a => a.ServiceName),
                ("date", "asc") => appointments.OrderBy(a => a.AppointmentDate),
                ("date", "desc") => appointments.OrderByDescending(a => a.AppointmentDate),
                ("status", "asc") => appointments.OrderBy(a => a.Status),
                ("status", "desc") => appointments.OrderByDescending(a => a.Status),
                _ => appointments.OrderBy(a => a.AppointmentDate)
            };

            ViewBag.SortColumn = col;
            ViewBag.SortDir = dir;
            ViewBag.CustomerSort = col == "customer" && dir == "asc" ? "customer_desc" : "customer_asc";
            ViewBag.ServiceSort = col == "service" && dir == "asc" ? "service_desc" : "service_asc";
            ViewBag.DateSort = col == "date" && dir == "asc" ? "date_desc" : "date_asc";
            ViewBag.StatusSort = col == "status" && dir == "asc" ? "status_desc" : "status_asc";

            return View(appointments.ToList());
        }
        public async Task<IActionResult> Upcoming(string sortOrder)
        {
            var user = await _userManager.GetUserAsync(User);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Id);
            var allAppointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employee.Id);

            var upcoming = allAppointments
                .Where(a => a.AppointmentDate > DateTime.Now &&
                            (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending));

            var parts = (sortOrder ?? "").Split('_');
            var col = parts.ElementAtOrDefault(0) ?? "";
            var dir = parts.ElementAtOrDefault(1) ?? "asc";

            upcoming = (col, dir) switch
            {
                ("service", "asc") => upcoming.OrderBy(a => a.ServiceName),
                ("service", "desc") => upcoming.OrderByDescending(a => a.ServiceName),
                ("customer", "asc") => upcoming.OrderBy(a => a.CustomerName),
                ("customer", "desc") => upcoming.OrderByDescending(a => a.CustomerName),
                ("date", "asc") => upcoming.OrderBy(a => a.AppointmentDate),
                ("date", "desc") => upcoming.OrderByDescending(a => a.AppointmentDate),
                ("status", "asc") => upcoming.OrderBy(a => a.Status),
                ("status", "desc") => upcoming.OrderByDescending(a => a.Status),
                ("price", "asc") => upcoming.OrderBy(a => a.Price),
                ("price", "desc") => upcoming.OrderByDescending(a => a.Price),
                _ => upcoming.OrderBy(a => a.AppointmentDate)
            };

            ViewBag.SortColumn = col;
            ViewBag.SortDir = dir;
            ViewBag.ServiceSort = col == "service" && dir == "asc" ? "service_desc" : "service_asc";
            ViewBag.CustomerSort = col == "customer" && dir == "asc" ? "customer_desc" : "customer_asc";
            ViewBag.DateSort = col == "date" && dir == "asc" ? "date_desc" : "date_asc";
            ViewBag.StatusSort = col == "status" && dir == "asc" ? "status_desc" : "status_asc";
            ViewBag.PriceSort = col == "price" && dir == "asc" ? "price_desc" : "price_asc";


            return View(upcoming.ToList());
        }

        public async Task<IActionResult> Past(string sortOrder)
        {
            var user = await _userManager.GetUserAsync(User);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Id);
            var allAppointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employee.Id);

            var past = allAppointments.Where(a => a.AppointmentDate < DateTime.Now);

            var parts = (sortOrder ?? "").Split('_');
            var col = parts.ElementAtOrDefault(0) ?? "";
            var dir = parts.ElementAtOrDefault(1) ?? "asc";

            past = (col, dir) switch
            {
                ("service", "asc") => past.OrderBy(a => a.ServiceName),
                ("service", "desc") => past.OrderByDescending(a => a.ServiceName),
                ("customer", "asc") => past.OrderBy(a => a.CustomerName),
                ("customer", "desc") => past.OrderByDescending(a => a.CustomerName),
                ("date", "asc") => past.OrderBy(a => a.AppointmentDate),
                ("date", "desc") => past.OrderByDescending(a => a.AppointmentDate),
                ("status", "asc") => past.OrderBy(a => a.Status),
                ("status", "desc") => past.OrderByDescending(a => a.Status),
                ("price", "asc") => past.OrderBy(a => a.Price),
                ("price", "desc") => past.OrderByDescending(a => a.Price),
                _ => past.OrderByDescending(a => a.AppointmentDate)
            };

            ViewBag.SortColumn = col;
            ViewBag.SortDir = dir;
            ViewBag.ServiceSort = col == "service" && dir == "asc" ? "service_desc" : "service_asc";
            ViewBag.CustomerSort = col == "customer" && dir == "asc" ? "customer_desc" : "customer_asc";
            ViewBag.DateSort = col == "date" && dir == "asc" ? "date_desc" : "date_asc";
            ViewBag.StatusSort = col == "status" && dir == "asc" ? "status_desc" : "status_asc";
            ViewBag.PriceSort = col == "price" && dir == "asc" ? "price_desc" : "price_asc";

            return View(past.ToList());
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
        [ValidateAntiForgeryToken]
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