using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using OnlineRandevuSistemi.Web.ViewModels;

namespace OnlineRandevuSistemi.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class HomeController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IServiceService _serviceService;

        public HomeController(IAppointmentService appointmentService, IServiceService serviceService)
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
        }

        public async Task<IActionResult> Index()
        {
            var allAppointments = await _appointmentService.GetAllAppointmentsAsync();

            var popularServiceGroups = allAppointments
                .GroupBy(a => a.ServiceId)
                .OrderByDescending(g => g.Count())
                .Take(5) // en çok alınan 5 hizmet
                .ToList();

            var popularServices = new List<PopularServiceViewModel>();

            foreach (var group in popularServiceGroups)
            {
                var service = await _serviceService.GetServiceByIdAsync(group.Key);
                if (service != null)
                {
                    popularServices.Add(new PopularServiceViewModel
                    {
                        Id = service.Id,
                        Name = service.Name,
                        AppointmentCount = group.Count()
                    });
                }
            }

            var model = new CustomerDashboardViewModel
            {
                PopularServices = popularServices
            };

            return View(model);
        }
    }
}