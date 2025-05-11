using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;

namespace OnlineRandevuSistemi.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/services")]
    [Authorize(Roles = "Admin")]
    public class ServiceApiController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServiceApiController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceService.GetAllServicesAsync();
            return Ok(services);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            return service == null ? NotFound() : Ok(service);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _serviceService.CreateServiceAsync(model);
            return Ok(new { message = "Hizmet başarıyla oluşturuldu." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceDto model)
        {
            if (id != model.Id)
                return BadRequest("ID uyuşmuyor.");

            await _serviceService.UpdateServiceAsync(model);
            return Ok(new { message = "Hizmet güncellendi." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _serviceService.DeleteServiceAsync(id);
            return Ok(new { message = "Hizmet silindi." });
        }
    }
}