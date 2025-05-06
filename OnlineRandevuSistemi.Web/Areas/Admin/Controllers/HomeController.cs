using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Enums;
using OnlineRandevuSistemi.Web.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
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

            var model = new AdminDashboardViewModel
            {
                TotalAppointments = appointments.Count(),
                ConfirmedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                TotalCustomers = (await _customerService.GetAllCustomersAsync()).Count(),
                TotalEmployees = (await _employeeService.GetAllEmployeesAsync()).Count(),
                TotalServices = (await _serviceService.GetAllServicesAsync()).Count()
            };

            return View(model);
        }
    }
}