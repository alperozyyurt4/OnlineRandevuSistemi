using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using OnlineRandevuSistemi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnlineRandevuSistemi.Web.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class NotificationController : Controller
    {
        private readonly IRepository<Notification> _notificationRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(IRepository<Notification> notificationRepository, UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var notifications = await _notificationRepository.TableNoTracking
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                await _notificationRepository.UpdateAsync(notification);
                await _unitOfWork.SaveChangesAsync();

            }

            return RedirectToAction("Index");
        }
    }
}