using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Enums;
using OnlineRandevuSistemi.Core.Interfaces;
using OnlineRandevuSistemi.Web.ViewModels;


namespace OnlineRandevuSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NotificationController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(
            UserManager<AppUser> userManager,
            IRepository<Notification> notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(string sortOrder)
        {
            var query = _notificationRepository.TableNoTracking.Include(n => n.User).AsQueryable();

            string currentColumn = "";
            string currentDirection = "asc";

            query = sortOrder switch
            {
                "date_desc" => query.OrderByDescending(n => n.CreatedDate),
                "date_asc" => query.OrderBy(n => n.CreatedDate),
                "user_asc" => query.OrderBy(n => n.User.FirstName),
                "user_desc" => query.OrderByDescending(n => n.User.FirstName),
                "title_asc" => query.OrderBy(n => n.Title),
                "title_desc" => query.OrderByDescending(n => n.Title),
                "type_asc" => query.OrderBy(n => n.Type),
                "type_desc" => query.OrderByDescending(n => n.Type),
                "read_asc" => query.OrderBy(n => n.IsRead),
                "read_desc" => query.OrderByDescending(n => n.IsRead),
                _ => query.OrderByDescending(n => n.CreatedDate)
            };

            if (!string.IsNullOrEmpty(sortOrder))
            {
                var parts = sortOrder.Split('_');
                currentColumn = parts[0];
                currentDirection = parts.Length > 1 && parts[1] == "desc" ? "desc" : "asc";
            }

            ViewBag.CurrentSortColumn = currentColumn;
            ViewBag.CurrentSortDirection = currentDirection;

            ViewBag.DateSort = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewBag.UserSort = sortOrder == "user_desc" ? "user_asc" : "user_desc";
            ViewBag.TitleSort = sortOrder == "title_desc" ? "title_asc" : "title_desc";
            ViewBag.TypeSort = sortOrder == "type_desc" ? "type_asc" : "type_desc";
            ViewBag.ReadSort = sortOrder == "read_desc" ? "read_asc" : "read_desc";

            var notifications = await query.ToListAsync();
            return View(notifications);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(NotificationCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Hedef kullanıcıları getir
            List<AppUser> users = new();

            if (model.TargetRole == "All")
                users = _userManager.Users.ToList();
            else
                users = (await _userManager.GetUsersInRoleAsync(model.TargetRole)).ToList();

            foreach (var user in users)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = user.Id,
                    Title = model.Title,
                    Message = model.Message,
                    Type = model.Type,
                    CreatedDate = DateTime.Now
                });
            }

            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Bildirimler gönderildi.";
            return RedirectToAction("Create");
        }
    }
}