using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class CustomerAppointmentCreateViewModel
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string SelectedTime { get; set; } // Saat string olarak gelecek, örn: "10:00"

        public string Notes { get; set; }

        public WeeklyAvailabilityViewModel? Availability { get; set; }

        public List<SelectListItem> Services { get; set; } = new();
        public List<SelectListItem> Employees { get; set; } = new();
    }
}