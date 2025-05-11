using OnlineRandevuSistemi.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class EmployeeWorkingHourViewModel
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        [Display(Name = "Gün")]
        public DayOfWeek DayOfWeek { get; set; }

        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Çalışıyor mu?")]
        public bool IsWorkingDay { get; set; }
    }
}