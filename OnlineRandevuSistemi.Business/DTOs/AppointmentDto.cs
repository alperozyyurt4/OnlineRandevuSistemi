using OnlineRandevuSistemi.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int EmployeeId { get; set; }
        public int CustomerId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime AppointmentEndTime { get; set; }
        public string Notes { get; set; }
        public AppointmentStatus Status { get; set; }
        public decimal Price { get; set; }
        public bool ReminderSent { get; set; }

        public string ServiceName { get; set; }
        public string EmployeeName { get; set; }
        public string CustomerName { get; set; }

    }
}
