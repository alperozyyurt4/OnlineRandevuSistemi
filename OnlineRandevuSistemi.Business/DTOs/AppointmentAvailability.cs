using System;
using System.Collections.Generic;

namespace OnlineRandevuSistemi.Business.DTOs
{
    public class DailySlotDto
    {
        public DateTime Date { get; set; }
        public List<TimeSlotDto> TimeSlots { get; set; } = new();
    }

    public class TimeSlotDto
    {
        public TimeSpan Time { get; set; }
        public bool IsAvailable { get; set; }
    }
  
    }