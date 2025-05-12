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

        public async Task<IActionResult> Index()
        {
            var notifications = await _notificationRepository.TableNoTracking
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

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