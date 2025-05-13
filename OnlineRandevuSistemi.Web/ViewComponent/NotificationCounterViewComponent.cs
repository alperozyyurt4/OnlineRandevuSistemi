using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Web.ViewComponents
{
    public class NotificationCounterViewComponent : ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Notification> _notificationRepository;

        public NotificationCounterViewComponent(UserManager<AppUser> userManager, IRepository<Notification> notificationRepository)
        {
            _userManager = userManager;
            _notificationRepository = notificationRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null) return Content("");

            var count = _notificationRepository.TableNoTracking
                .Count(n => n.UserId == user.Id && !n.IsRead);

            return View("Default", count);
        }
    }
}