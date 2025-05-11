using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Interfaces;

namespace OnlineRandevuSistemi.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/working-hours")]
    [Authorize(Roles = "Admin")]
    public class WorkingHourApiController : ControllerBase
    {
        private readonly IRepository<WorkingHour> _workingHourRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WorkingHourApiController(IRepository<WorkingHour> workingHourRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _workingHourRepository = workingHourRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: Çalışanın tüm çalışma saatleri
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var hours = await _workingHourRepository.TableNoTracking
                .Where(w => w.EmployeeId == employeeId)
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();

            var dtos = hours.Select(h => _mapper.Map<EmployeeWorkingHourDto>(h)).ToList();
            return Ok(dtos);
        }

        // GET: Tek bir çalışma saati
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var entity = await _workingHourRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            var dto = _mapper.Map<EmployeeWorkingHourDto>(entity);
            return Ok(dto);
        }

        // PUT: Güncelleme işlemi
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeWorkingHourDto model)
        {
            if (id != model.Id)
                return BadRequest("ID uyuşmuyor.");

            var entity = await _workingHourRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            _mapper.Map(model, entity);
            await _workingHourRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Çalışma saati güncellendi." });
        }
    }
}