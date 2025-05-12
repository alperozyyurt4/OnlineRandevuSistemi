using OnlineRandevuSistemi.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Web.ViewModels
{
    public class NotificationCreateViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        public string TargetRole { get; set; } = "Customer"; // Customer, Employee, All
    }
}