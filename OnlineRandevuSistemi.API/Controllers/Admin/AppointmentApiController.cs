using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Enums;

namespace OnlineRandevuSistemi.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/appointments")]
    [Authorize(Roles = "Admin")]
    public class AppointmentApiController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IServiceService _serviceService;
        private readonly IEmployeeService _employeeService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<AppointmentApiController> _logger;

        public AppointmentApiController(
            IAppointmentService appointmentService,
            IServiceService serviceService,
            IEmployeeService employeeService,
            ICustomerService customerService,
            ILogger<AppointmentApiController> logger)
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
            _employeeService = employeeService;
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? sort)
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();

            appointments = sort switch
            {
                "date_desc" => appointments.OrderByDescending(a => a.AppointmentDate).ToList(),
                "date_asc" => appointments.OrderBy(a => a.AppointmentDate).ToList(),
                _ => appointments.OrderByDescending(a => a.AppointmentDate).ToList()
            };

            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound();

            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
        {
            try
            {
                await _appointmentService.CreateAppointmentAsync(dto);
                return Ok(new { message = "Randevu oluşturuldu." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create error: {ex.Message}");
                return BadRequest("Randevu oluşturulamadı.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID uyuşmuyor.");

            try
            {
                await _appointmentService.UpdateAppointmentAsync(dto);
                return Ok(new { message = "Randevu güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update error: {ex.Message}");
                return BadRequest("Randevu güncellenemedi.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _appointmentService.DeleteAppointmentAsync(id);
            if (!result)
                return BadRequest("Silinemedi.");

            return Ok(new { message = "Randevu silindi." });
        }

        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] AppointmentStatusUpdateDto model)
        {
            var success = await _appointmentService.UpdateAppointmentStatusAsync(model.AppointmentId, model.Status);
            return success ? Ok("Durum güncellendi.") : BadRequest("Durum güncellenemedi.");
        }

        [HttpGet("employees-by-service/{serviceId}")]
        public async Task<IActionResult> GetEmployeesByService(int serviceId)
        {
            var employees = await _employeeService.GetEmployeesByServiceIdAsync(serviceId);
            return Ok(employees.Select(e => new
            {
                e.Id,
                FullName = $"{e.FirstName} {e.LastName}"
            }));
        }
    }

    public class AppointmentStatusUpdateDto
    {
        public int AppointmentId { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}