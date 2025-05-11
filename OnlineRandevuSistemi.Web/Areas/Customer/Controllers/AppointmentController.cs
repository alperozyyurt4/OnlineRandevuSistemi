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
using AutoMapper;

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
        private readonly IMapper _mapper;

        public AppointmentController(IAppointmentService appointmentService, UserManager<AppUser> userManager, ICustomerService customerService, IServiceService serviceService, IEmployeeService employeeService, IMapper mapper)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
            _customerService = customerService;
            _serviceService = serviceService;
            _employeeService = employeeService;
            _mapper = mapper;
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

                Employees = new List<SelectListItem>(),

                AppointmentDate = DateTime.Now.AddDays(1),
                Availability = null

            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerAppointmentCreateViewModel model)
        {
            if (model.AppointmentDate < DateTime.Now)
            {
                ModelState.AddModelError("Appointment Date", "Geçmiş bir tarih seçilemez");

            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    foreach (var e in error.Value.Errors)
                    {
                        Console.WriteLine($"Hata: {error.Key} - {e.ErrorMessage}");
                    }
                }
                model.Services = (await _serviceService.GetAllServicesAsync())
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();

                model.Employees = (await _employeeService.GetAllEmployeesAsync())
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToList();

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var customer = await _customerService.GetCustomerByUserIdAsync(user.Id);

            var appointmentDate = DateTime.Parse($"{model.AppointmentDate:yyyy-MM-dd} {model.SelectedTime}");

            var dto = new AppointmentCreateDto
            {
                ServiceId = model.ServiceId,
                EmployeeId = model.EmployeeId,
                CustomerId = customer.Id,
                AppointmentDate = appointmentDate,
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
        [HttpGet]
        public async Task<IActionResult> GetAvailableHours(int employeeId, DateTime appointmentDate)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                return Json(new List<string>());

            var workingHours = employee.WorkingHours
                .FirstOrDefault(w => w.DayOfWeek == appointmentDate.DayOfWeek && w.IsWorkingDay);

            if (workingHours == null)
                return Json(new List<string>());

            var start = workingHours.StartTime;
            var end = workingHours.EndTime;

            var service = await _serviceService.GetServiceByIdAsync(employee.ServiceIds.FirstOrDefault());
            int duration = service?.DurationMinutes ?? 30;

            var appointments = await _appointmentService.GetAppointmentsByEmployeeIdAsync(employeeId);
            var occupiedSlots = appointments
                .Where(a => a.AppointmentDate.Date == appointmentDate.Date)
                .Select(a => new
                {
                    Start = a.AppointmentDate.TimeOfDay,
                    End = a.AppointmentEndTime.TimeOfDay
                }).ToList();

            var availableHours = new List<string>();
            for (var time = start; time.Add(TimeSpan.FromMinutes(duration)) <= end; time = time.Add(TimeSpan.FromMinutes(duration)))
            {
                var endTime = time.Add(TimeSpan.FromMinutes(duration));
                bool isFree = !occupiedSlots.Any(a =>
                    (time >= a.Start && time < a.End) || (endTime > a.Start && endTime <= a.End));

                if (isFree)
                    availableHours.Add(time.ToString(@"hh\:mm"));
            }

            return Json(availableHours);
        }
        [HttpGet]
        public async Task<IActionResult> GetWeeklyAvailability(int employeeId, string selectedDate)
        {
            try
            {
                if (!DateTime.TryParse(selectedDate, out DateTime startDate))
                    startDate = DateTime.Today;

                var endDate = startDate.AddDays(6);

                var dailyDtos = await _appointmentService.GetWeeklyAvailabilityAsync(employeeId, startDate, endDate);
                var days = _mapper.Map<List<DailySlotViewModel>>(dailyDtos);

                var viewModel = new WeeklyAvailabilityViewModel
                {
                    EmployeeId = employeeId,
                    Days = days
                };

                return PartialView("_WeeklyAvailabilityPartial", viewModel);
            }
            catch (Exception ex)
            {
                // Hatayı loglayabilirsiniz
                Console.WriteLine($"GetWeeklyAvailability Error: {ex.Message}");
                return Content("<p class='text-danger'>Uygun saatler yüklenirken bir hata oluştu.</p>");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetTimeSlotsForDate(int employeeId, string selectedDate)
        {
            if (!DateTime.TryParse(selectedDate, out var date))
                return Content("<p class='text-danger'>Geçersiz tarih</p>");

            var dailyDto = await _appointmentService.GetDailyAvailabilityAsync(employeeId, date);
            var slots = dailyDto?.TimeSlots ?? new List<TimeSlotDto>();

            return PartialView("_TimeSlotPartial", slots);
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