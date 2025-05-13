using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;

        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        public async Task<IActionResult> Index(string sortOrder)
        {
            var services = await _serviceService.GetAllServicesAsync();

            string currentColumn = "";
            string currentDirection = "asc";

            services = sortOrder switch
            {
                "name_desc" => services.OrderByDescending(s => s.Name),
                "name_asc" => services.OrderBy(s => s.Name),
                "price_desc" => services.OrderByDescending(s => s.Price),
                "price_asc" => services.OrderBy(s => s.Price),
                "duration_desc" => services.OrderByDescending(s => s.DurationMinutes),
                "duration_asc" => services.OrderBy(s => s.DurationMinutes),
                _ => services.OrderBy(s => s.Name) // default: Name ascending
            };

            if (!string.IsNullOrEmpty(sortOrder))
            {
                var parts = sortOrder.Split('_');
                currentColumn = parts[0];
                currentDirection = parts.Length > 1 && parts[1] == "desc" ? "desc" : "asc";
            }

            ViewBag.CurrentSortColumn = currentColumn;
            ViewBag.CurrentSortDirection = currentDirection;

            ViewBag.NameSort = sortOrder == "name_desc" ? "name_asc" : "name_desc";
            ViewBag.PriceSort = sortOrder == "price_desc" ? "price_asc" : "price_desc";
            ViewBag.DurationSort = sortOrder == "duration_desc" ? "duration_asc" : "duration_desc";

            return View(services);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceDto model)
        {
            if (ModelState.IsValid)
            {
                await _serviceService.CreateServiceAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
                return NotFound();

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceDto model)
        {
            if (ModelState.IsValid)
            {
                await _serviceService.UpdateServiceAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _serviceService.DeleteServiceAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
