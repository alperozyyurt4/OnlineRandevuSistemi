using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Enums;
using OnlineRandevuSistemi.Web.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly IServiceService _serviceService;

        public HomeController(
            IAppointmentService appointmentService,
            ICustomerService customerService,
            IEmployeeService employeeService,
            IServiceService serviceService)
        {
            _appointmentService = appointmentService;
            _customerService = customerService;
            _employeeService = employeeService;
            _serviceService = serviceService;
        }
        public async Task<IActionResult> Index()
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();

            var popularServiceGroup = appointments
                .GroupBy(x => x.ServiceId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToList();

            var popularServices = new List<PopularServiceViewModel>();

            foreach (var group in popularServiceGroup)
            {
                var service = await _serviceService.GetServiceByIdAsync(group.Key);
                if (service != null)
                {
                    popularServices.Add(new PopularServiceViewModel
                    {
                        Id = service.Id,
                        Name = service.Name,
                        AppointmentCount = group.Count(),
                    });
                    
                }

            }

            var model = new AdminDashboardViewModel
            {
                TotalAppointments = appointments.Count(),
                ConfirmedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                TotalCustomers = (await _customerService.GetAllCustomersAsync()).Count(),
                TotalEmployees = (await _employeeService.GetAllEmployeesAsync()).Count(),
                TotalServices = (await _serviceService.GetAllServicesAsync()).Count(),
                PopularServices = popularServices
            };

            return View(model);
        }
    }
}