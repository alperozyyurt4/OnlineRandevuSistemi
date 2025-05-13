using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Interfaces;

namespace OnlineRandevuSistemi.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
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
        public async Task<IActionResult> Index(string sortOrder)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var query = _notificationRepository.TableNoTracking
                .Where(n => n.UserId == currentUser.Id);

            string currentColumn = "";
            string currentDirection = "asc";

            query = sortOrder switch
            {
                "date_desc" => query.OrderByDescending(n => n.CreatedDate),
                "date_asc" => query.OrderBy(n => n.CreatedDate),
                "title_desc" => query.OrderByDescending(n => n.Title),
                "title_asc" => query.OrderBy(n => n.Title),
                "type_desc" => query.OrderByDescending(n => n.Type),
                "type_asc" => query.OrderBy(n => n.Type),
                "read_desc" => query.OrderByDescending(n => n.IsRead),
                "read_asc" => query.OrderBy(n => n.IsRead),
                _ => query.OrderByDescending(n => n.CreatedDate)
            };

            if (!string.IsNullOrEmpty(sortOrder))
            {
                var parts = sortOrder.Split('_');
                currentColumn = parts[0];
                currentDirection = parts.Length > 1 ? parts[1] : "asc";
            }

            ViewBag.CurrentSortColumn = currentColumn;
            ViewBag.CurrentSortDirection = currentDirection;

            ViewBag.DateSort = currentColumn == "date" && currentDirection == "asc" ? "date_desc" : "date_asc";
            ViewBag.TitleSort = currentColumn == "title" && currentDirection == "asc" ? "title_desc" : "title_asc";
            ViewBag.TypeSort = currentColumn == "type" && currentDirection == "asc" ? "type_desc" : "type_asc";
            ViewBag.ReadSort = currentColumn == "read" && currentDirection == "asc" ? "read_desc" : "read_asc";

            var notifications = await query.ToListAsync();
            return View(notifications);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification != null && !notification.IsRead)
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