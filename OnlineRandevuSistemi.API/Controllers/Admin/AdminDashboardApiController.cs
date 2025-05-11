using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Enums;

namespace OnlineRandevuSistemi.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardApiController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly IServiceService _serviceService;

        public AdminDashboardApiController(
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

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();

            var result = new
            {
                TotalAppointments = appointments.Count(),
                ConfirmedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                TotalCustomers = (await _customerService.GetAllCustomersAsync()).Count(),
                TotalEmployees = (await _employeeService.GetAllEmployeesAsync()).Count(),
                TotalServices = (await _serviceService.GetAllServicesAsync()).Count(),
                PopularServices = await _serviceService.GetPopularServicesAsync()
            };

            return Ok(result);
        }
    }
}