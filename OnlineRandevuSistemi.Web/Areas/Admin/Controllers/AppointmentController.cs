using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Enums;
using OnlineRandevuSistemi.Web.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IServiceService _serviceService;
        private readonly IEmployeeService _employeeService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(
            IAppointmentService appointmentService,
            IServiceService serviceService,
            IEmployeeService employeeService,
            ICustomerService customerService,
            ILogger<AppointmentController> logger
            )
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
            _employeeService = employeeService;
            _customerService = customerService;
            _logger = logger;
        }
        public async Task<IActionResult> Index(string? sort)
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();

            if (!string.IsNullOrEmpty(sort))
            {
                appointments = sort switch
                {
                    "date_desc" => appointments.OrderByDescending(a => a.AppointmentDate).ToList(),
                    "date_asc" => appointments.OrderBy(a => a.AppointmentDate).ToList(),
                    _ => appointments
                };

                ViewBag.CurrentSort = sort;
            }
            else
            {
                // DEFAULT davranış burada belirlenir → örneğin en son randevular yukarıda
                appointments = appointments.OrderByDescending(a => a.AppointmentDate).ToList();
                ViewBag.CurrentSort = null;
            }

            return View(appointments);
        }

        // Detay Sayfası Action'ı
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    _logger.LogWarning($"Randevu bulunamadı: ID={id}");
                    return NotFound();
                }
                return View(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu detayları alınırken hata: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            var model = new AppointmentCreateViewModel
            {
                Services = (await _serviceService.GetAllServicesAsync())
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList(),
                Employees = new List<SelectListItem>(),
                Customers = (await _customerService.GetAllCustomersAsync())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FirstName + " " + c.LastName }).ToList(),
                AppointmentDate = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            // ModelState'den dropdown listelerini çıkar
            ModelState.Remove("Services");
            ModelState.Remove("Employees");
            ModelState.Remove("Customers");

            if (!ModelState.IsValid)
            {
                // Kalan validation hatalarını logla
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogError($"Error in {state.Key}: {error.ErrorMessage}");
                        }
                    }
                }

                // Dropdown verilerini yeniden yükle
                model.Services = (await _serviceService.GetAllServicesAsync())
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
                model.Employees = (await _employeeService.GetAllEmployeesAsync())
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToList();
                model.Customers = (await _customerService.GetAllCustomersAsync())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FirstName + " " + c.LastName }).ToList();

                return View(model);
            }

            var dto = new AppointmentCreateDto
            {
                ServiceId = model.ServiceId,
                EmployeeId = model.EmployeeId,
                CustomerId = model.CustomerId,
                AppointmentDate = model.AppointmentDate,
                Notes = model.Notes
            };

            try
            {
                await _appointmentService.CreateAppointmentAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu oluşturulurken hata: {ex.Message}");
                ModelState.AddModelError("", "Randevu oluşturulurken bir hata oluştu: " + ex.Message);

                // Dropdown verilerini yeniden yükle
                model.Services = (await _serviceService.GetAllServicesAsync())
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
                model.Employees = (await _employeeService.GetAllEmployeesAsync())
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToList();
                model.Customers = (await _customerService.GetAllCustomersAsync())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FirstName + " " + c.LastName }).ToList();

                return View(model);
            }
        }

        // GET Edit
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound();

            var model = new AppointmentCreateViewModel
            {
                Id = appointment.Id,
                ServiceId = appointment.ServiceId,
                EmployeeId = appointment.EmployeeId,
                CustomerId = appointment.CustomerId,
                AppointmentDate = appointment.AppointmentDate,
                Notes = appointment.Notes,
                Services = (await _serviceService.GetAllServicesAsync())
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList(),
                Employees = (await _employeeService.GetAllEmployeesAsync())
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToList(),
                Customers = (await _customerService.GetAllCustomersAsync())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FirstName + " " + c.LastName }).ToList()
            };

            return View(model);
        }

        // POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppointmentCreateViewModel model)
        {
            // ModelState'den dropdown listelerini çıkar
            ModelState.Remove("Services");
            ModelState.Remove("Employees");
            ModelState.Remove("Customers");

            if (!ModelState.IsValid)
            {
                // Kalan validation hatalarını logla
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogError($"Error in {state.Key}: {error.ErrorMessage}");
                        }
                    }
                }

                // Dropdown verilerini yeniden yükle
                model.Services = (await _serviceService.GetAllServicesAsync())
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
                model.Employees = (await _employeeService.GetAllEmployeesAsync())
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToList();
                model.Customers = (await _customerService.GetAllCustomersAsync())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FirstName + " " + c.LastName }).ToList();

                return View(model);
            }

            var dto = new AppointmentUpdateDto
            {
                Id = model.Id,
                ServiceId = model.ServiceId,
                EmployeeId = model.EmployeeId,
                CustomerId = model.CustomerId,
                AppointmentDate = model.AppointmentDate,
                Notes = model.Notes
            };

            try
            {
                await _appointmentService.UpdateAppointmentAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Randevu güncellenirken hata: {ex.Message}");
                ModelState.AddModelError("", "Randevu güncellenirken bir hata oluştu: " + ex.Message);

                // Dropdown verilerini yeniden yükle
                model.Services = (await _serviceService.GetAllServicesAsync())
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
                model.Employees = (await _employeeService.GetAllEmployeesAsync())
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToList();
                model.Customers = (await _customerService.GetAllCustomersAsync())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FirstName + " " + c.LastName }).ToList();

                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _appointmentService.DeleteAppointmentAsync(id);

            if (!result)
            {
                _logger.LogWarning($"Silme işlemi başarısız oldu. ID: {id}");
                // Hata mesajı gösterilebilir
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            var success = await _appointmentService.UpdateAppointmentStatusAsync(id, status);
            if (!success)
            {
                TempData["Error"] = "Randevu durumu güncellenemedi.";
            }
            else
            {
                TempData["Success"] = "Randevu durumu başarıyla güncellendi.";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByServiceId(int serviceId)
        {
            var employees = await _employeeService.GetEmployeesByServiceIdAsync(serviceId);
            var items = employees.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.FirstName} {e.LastName}"
            }).ToList();

            return Json(items);
        }

    }
}   