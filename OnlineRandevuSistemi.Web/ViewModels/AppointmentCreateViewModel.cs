using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace OnlineRandevuSistemi.Web.ViewModels
{
    public class AppointmentCreateViewModel
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet seçilmesi gerekmektedir.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Çalışan seçilmesi gerekmektedir.")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Müşteri seçilmesi gerekmektedir.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi gerekmektedir.")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Randevu saati gerekmektedir.")]
        public string SelectedTime { get; set; } // Saat string olarak gelecek, örn: "10:00"

        public string Notes { get; set; }

        public WeeklyAvailabilityViewModel? Availability { get; set; }

        // Add [BindNever] or exclude these from model binding
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<SelectListItem> Services { get; set; } = new List<SelectListItem>();

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<SelectListItem> Customers { get; set; } = new List<SelectListItem>();
    }
}