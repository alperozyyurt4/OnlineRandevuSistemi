using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.Interfaces;

namespace OnlineRandevuSistemi.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/dashboard")]
    [Authorize(Roles = "Customer")]
    public class DashboardApiController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IServiceService _serviceService;

        public DashboardApiController(IAppointmentService appointmentService, IServiceService serviceService)
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
        }

        [HttpGet("popular-services")]
        public async Task<IActionResult> GetPopularServices()
        {
            var allAppointments = await _appointmentService.GetAllAppointmentsAsync();

            var popularServiceGroups = allAppointments
                .GroupBy(a => a.ServiceId)
                .OrderByDescending(g => g.Count())
                .Take(5) // en çok alınan 5 hizmet
                .ToList();

            var result = new List<PopularServiceDto>();

            foreach (var group in popularServiceGroups)
            {
                var service = await _serviceService.GetServiceByIdAsync(group.Key);
                if (service != null)
                {
                    result.Add(new PopularServiceDto
                    {
                        Id = service.Id,
                        Name = service.Name,
                        AppointmentCount = group.Count()
                    });
                }
            }

            return Ok(result);
        }
    }

    public class PopularServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AppointmentCount { get; set; }
    }
}