using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OnlineRandevuSistemi.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Services;
using OnlineRandevuSistemi.Web.ViewModels;
using OnlineRandevuSistemi.Core.Enums;

namespace OnlineRandevuSistemi.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICustomerService _customerService;
        private readonly IServiceService _serviceService;
        private readonly IEmployeeService _employeeService;

        public AppointmentController(IAppointmentService appointmentService, UserManager<AppUser> userManager, ICustomerService customerService, IServiceService serviceService, IEmployeeService employeeService)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
            _customerService = customerService;
            _serviceService = serviceService;
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _customerService.GetCustomerByUserIdAsync(user.Id);
            var allAppointments = await _appointmentService.GetAppointmentsByCustomerIdAsync(customer.Id);

            var now = DateTime.Now;

            var model = new AppointmentListViewModel
            {
                UpcomingAppointments = allAppointments
                    .Where(a => a.AppointmentDate >= now)
                    .OrderBy(a => a.AppointmentDate)
                    .ToList(),

                PastAppointments = allAppointments
                    .Where(a => a.AppointmentDate < now)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToList()
            };

            return View(model);
        }
        public async Task<IActionResult> Create()
        {
            var model = new CustomerAppointmentCreateViewModel
            {
                Services = (await _serviceService.GetAllServicesAsync())
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList(),

                Employees = (await _employeeService.GetAllEmployeesAsync())
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToList(),

                AppointmentDate = DateTime.Now.AddDays(1)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerAppointmentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Services = (await _serviceService.GetAllServicesAsync())
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();

                model.Employees = (await _employeeService.GetAllEmployeesAsync())
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToList();

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var customer = await _customerService.GetCustomerByUserIdAsync(user.Id);

            var dto = new AppointmentCreateDto
            {
                ServiceId = model.ServiceId,
                EmployeeId = model.EmployeeId,
                CustomerId = customer.Id,
                AppointmentDate = model.AppointmentDate,
                Notes = model.Notes
            };

            await _appointmentService.CreateAppointmentAsync(dto);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound();
            var user = await _userManager.GetUserAsync(User);
            var customer = await _customerService.GetCustomerByUserIdAsync(user.Id);

            if (appointment.CustomerId != customer.Id)
                return Forbid();

            return View(appointment);
        }
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if(appointment == null)
                    return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var customer = await _customerService.GetCustomerByUserIdAsync(user.Id);

            if (appointment.CustomerId != customer.Id)
                return Forbid();
           

            await _appointmentService.UpdateAppointmentStatusAsync(id, AppointmentStatus.Cancelled);
            return RedirectToAction(nameof(Index));
        }
    }
}