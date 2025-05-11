using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Core.Interfaces;
using OnlineRandevuSistemi.Web.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace OnlineRandevuSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class WorkingHourController : Controller
    {
        private readonly IRepository<WorkingHour> _workingHourRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public WorkingHourController(IRepository<WorkingHour> workingHourRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _workingHourRepository = workingHourRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int employeeId)
        {
            var hours = await _workingHourRepository.TableNoTracking
                .Where(w => w.EmployeeId == employeeId)
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();

            ViewBag.EmployeeId = employeeId;
            var dtos = hours.Select(h => _mapper.Map<EmployeeWorkingHourDto>(h)).ToList();
            return View(dtos);
        }
        // GET
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _workingHourRepository.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            var viewModel = new EmployeeWorkingHourViewModel
            {
                Id = dto.Id,
                EmployeeId = dto.EmployeeId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsWorkingDay = dto.IsWorkingDay
            };

            return View(viewModel);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeWorkingHourDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var hour = await _workingHourRepository.GetByIdAsync(model.Id);
            if (hour == null) return NotFound();

            _mapper.Map(model, hour);
            await _workingHourRepository.UpdateAsync(hour);
            await _unitOfWork.SaveChangesAsync();

            return RedirectToAction("Index", new { employeeId = hour.EmployeeId });
        }
    }
}