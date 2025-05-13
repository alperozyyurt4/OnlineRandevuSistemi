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
        public async Task<IActionResult> Index(string sortOrder)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _notificationRepository.TableNoTracking
                .Where(n => n.UserId == user.Id);

            string col = "", dir = "asc";

            if (!string.IsNullOrEmpty(sortOrder))
            {
                var parts = sortOrder.Split('_');
                col = parts[0];
                dir = parts.Length > 1 ? parts[1] : "asc";
            }

            query = (col, dir) switch
            {
                ("date", "asc") => query.OrderBy(n => n.CreatedDate),
                ("date", "desc") => query.OrderByDescending(n => n.CreatedDate),
                ("title", "asc") => query.OrderBy(n => n.Title),
                ("title", "desc") => query.OrderByDescending(n => n.Title),
                ("type", "asc") => query.OrderBy(n => n.Type),
                ("type", "desc") => query.OrderByDescending(n => n.Type),
                ("read", "asc") => query.OrderBy(n => n.IsRead),
                ("read", "desc") => query.OrderByDescending(n => n.IsRead),
                _ => query.OrderByDescending(n => n.CreatedDate)
            };

            ViewBag.CurrentSortColumn = col;
            ViewBag.CurrentSortDirection = dir;
            ViewBag.DateSort = col == "date" && dir == "asc" ? "date_desc" : "date_asc";
            ViewBag.TitleSort = col == "title" && dir == "asc" ? "title_desc" : "title_asc";
            ViewBag.TypeSort = col == "type" && dir == "asc" ? "type_desc" : "type_asc";
            ViewBag.ReadSort = col == "read" && dir == "asc" ? "read_desc" : "read_asc";

            var notifications = await query.ToListAsync();
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