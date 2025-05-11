using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Api.ViewModels;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Enums;
using System.Security.Claims;

namespace OnlineRandevuSistemi.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/appointments")]
    [Authorize(Roles = "Customer")]
    public class AppointmentApiController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICustomerService _customerService;
        private readonly IServiceService _serviceService;
        private readonly IEmployeeService _employeeService;

        public AppointmentApiController(
            IAppointmentService appointmentService,
            UserManager<AppUser> userManager,
            ICustomerService customerService,
            IServiceService serviceService,
            IEmployeeService employeeService)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
            _customerService = customerService;
            _serviceService = serviceService;
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
                return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByCustomerIdAsync(customer.Id);
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (appointment == null || appointment.CustomerId != customer.Id)
                return Forbid();

            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateRequestDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            var dto = new AppointmentCreateDto
            {
                CustomerId = customer.Id,
                EmployeeId = model.EmployeeId,
                ServiceId = model.ServiceId,
                AppointmentDate = model.AppointmentDate,
                Notes = model.Notes
            };

            await _appointmentService.CreateAppointmentAsync(dto);
            return Ok(new { message = "Randevu oluşturuldu." });
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (appointment == null || appointment.CustomerId != customer.Id)
                return Forbid();

            await _appointmentService.UpdateAppointmentStatusAsync(id, AppointmentStatus.Cancelled);
            return Ok(new { message = "Randevu iptal edildi." });
        }

        [HttpGet("available-hours")]
        public async Task<IActionResult> GetAvailableHours(int employeeId, DateTime appointmentDate)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                return Ok(new List<string>());

            var workingHours = employee.WorkingHours
                .FirstOrDefault(w => w.DayOfWeek == appointmentDate.DayOfWeek && w.IsWorkingDay);

            if (workingHours == null)
                return Ok(new List<string>());

            var service = await _serviceService.GetServiceByIdAsync(employee.ServiceIds.FirstOrDefault());
            var duration = service?.DurationMinutes ?? 30;

            var appointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employeeId);
            var occupiedSlots = appointments
                .Where(a => a.AppointmentDate.Date == appointmentDate.Date)
                .Select(a => new
                {
                    Start = a.AppointmentDate.TimeOfDay,
                    End = a.AppointmentEndTime.TimeOfDay
                }).ToList();

            var availableHours = new List<string>();
            for (var time = workingHours.StartTime;
                 time.Add(TimeSpan.FromMinutes(duration)) <= workingHours.EndTime;
                 time = time.Add(TimeSpan.FromMinutes(duration)))
            {
                var endTime = time.Add(TimeSpan.FromMinutes(duration));
                bool isFree = !occupiedSlots.Any(a =>
                    (time >= a.Start && time < a.End) || (endTime > a.Start && endTime <= a.End));

                if (isFree)
                    availableHours.Add(time.ToString(@"hh\:mm"));
            }

            return Ok(availableHours);
        }

        [HttpGet("employees-by-service/{serviceId}")]
        public async Task<IActionResult> GetEmployeesByServiceId(int serviceId)
        {
            var employees = await _employeeService.GetEmployeesByServiceIdAsync(serviceId);
            var result = employees.Select(e => new
            {
                e.Id,
                FullName = $"{e.FirstName} {e.LastName}"
            });

            return Ok(result);
        }
    }
    public class AppointmentCreateRequestDto
    {
        public int ServiceId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
    }
}