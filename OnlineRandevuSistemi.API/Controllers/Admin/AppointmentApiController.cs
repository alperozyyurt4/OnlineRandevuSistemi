﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

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

            dto.AppointmentDate = new DateTime(
                dto.AppointmentDate.Year,
                dto.AppointmentDate.Month,
                dto.AppointmentDate.Day,
                dto.AppointmentDate.Hour,
                dto.AppointmentDate.Minute,
                0
                );
            // 1. Geçmiş tarih kontrolü
            if (dto.AppointmentDate < DateTime.Now)
                return BadRequest("Geçmiş bir tarihe randevu oluşturulamaz.");

            // 2. SLOT kontrolü: Yalnızca 30 dakikalık aralıklara izin ver
            var minute = dto.AppointmentDate.Minute;
            if (minute % 30 != 0)
                return BadRequest("Randevular yalnızca 30 dakikalık dilimlerde alınabilir (örn: 09:00, 09:30)");

            // 3. Çalışan kontrolü
            var employee = await _employeeService.GetEmployeeByIdAsync(dto.EmployeeId);
            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            var workingHour = employee.WorkingHours
                .FirstOrDefault(w => w.DayOfWeek == dto.AppointmentDate.DayOfWeek && w.IsWorkingDay);

            if (workingHour == null)
                return BadRequest("Seçilen gün çalışan çalışmıyor.");

            // 4. Hizmet süresi kontrolü
            var service = await _serviceService.GetServiceByIdAsync(dto.ServiceId);
            int duration = service?.DurationMinutes ?? 30;

            var appointmentStart = dto.AppointmentDate.TimeOfDay;
            var appointmentEnd = appointmentStart + TimeSpan.FromMinutes(duration);

            // 5. Çalışma saatleri içinde mi?
            if (appointmentStart < workingHour.StartTime || appointmentEnd > workingHour.EndTime)
                return BadRequest("Seçilen saat, çalışanın çalışma saatleri dışında.");

            // 6. Mevcut randevularla çakışma kontrolü
            var existingAppointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(dto.EmployeeId);
            bool isConflicting = existingAppointments.Any(a =>
                a.AppointmentDate.Date == dto.AppointmentDate.Date &&
                dto.AppointmentDate < a.AppointmentEndTime &&
                dto.AppointmentDate.AddMinutes(duration) > a.AppointmentDate
            );

            if (isConflicting)
                return BadRequest("Seçilen saat diliminde bu çalışanın başka bir randevusu var.");

            // 7. Oluştur
            try
            {
                await _appointmentService.CreateAppointmentAsync(dto);
                return Ok(new { message = "Randevu başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu oluşturulurken hata.");
                return StatusCode(500, "Sunucu hatası.");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentUpdateDto dto)
        {
            if (dto.AppointmentDate < DateTime.Now)
                return BadRequest("Geçmiş bir tarihe güncelleme yapılamaz.");

            try
            {
                dto.Id = id;
                await _appointmentService.UpdateAppointmentAsync(dto);
                return Ok(new { message = "Randevu başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu güncellenirken hata.");
                return StatusCode(500, "Sunucu hatası.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _appointmentService.DeleteAppointmentAsync(id);
            if (!result)
                return NotFound(new { message = "Silinecek randevu bulunamadı." });

            return Ok(new { message = "Randevu başarıyla silindi." });
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] AppointmentStatus status)
        {
            var success = await _appointmentService.UpdateAppointmentStatusAsync(id, status);
            if (!success)
                return BadRequest("Randevu durumu güncellenemedi.");

            return Ok(new { message = "Durum güncellendi." });
        }

        [HttpGet("employees-by-service/{serviceId}")]
        public async Task<IActionResult> GetEmployeesByServiceId(int serviceId)
        {
            var employees = await _employeeService.GetEmployeesByServiceIdAsync(serviceId);
            var response = employees.Select(e => new
            {
                e.Id,
                FullName = $"{e.FirstName} {e.LastName}"
            });

            return Ok(response);
        }

        [HttpGet("weekly-availability")]
        public async Task<IActionResult> GetWeeklyAvailability(int employeeId, string selectedDate)
        {
            if (!DateTime.TryParse(selectedDate, out DateTime startDate))
                startDate = DateTime.Today;

            var endDate = startDate.AddDays(6);

            try
            {
                var data = await _appointmentService.GetWeeklyAvailabilityAsync(employeeId, startDate, endDate);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Haftalık uygunluk alınamadı.");
                return StatusCode(500, "Veri alınamadı.");
            }
        }

        [HttpGet("timeslots")]
        public async Task<IActionResult> GetTimeSlotsForDate(int employeeId, string selectedDate)
        {
            if (!DateTime.TryParse(selectedDate, out var date))
                return BadRequest("Geçersiz tarih.");

            var dailyDto = await _appointmentService.GetDailyAvailabilityAsync(employeeId, date);
            return Ok(dailyDto?.TimeSlots ?? new List<TimeSlotDto>());
        }
    }
}