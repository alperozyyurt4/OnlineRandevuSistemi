using OnlineRandevuSistemi.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class EmployeeAppointmentStatusViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Mevcut Durum")]
        public AppointmentStatus CurrentStatus { get; set; }

        [Required]
        [Display(Name = "Yeni Durum")]
        public AppointmentStatus NewStatus { get; set; }
    }
}