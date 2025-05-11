using System;
using System.Collections.Generic;

namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class WeeklyAvailabilityViewModel
    {
        public int EmployeeId { get; set; }
        public List<DailySlotViewModel> Days { get; set; } = new();
    }

    public class DailySlotViewModel
    {
        public DateTime Date { get; set; }
        public List<TimeSlotViewModel> TimeSlots { get; set; } = new();
    }

    public class TimeSlotViewModel
    {
        public TimeSpan Time { get; set; }
        public bool IsAvailable { get; set; }
    }
}