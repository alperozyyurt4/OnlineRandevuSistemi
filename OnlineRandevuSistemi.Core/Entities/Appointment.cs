using OnlineRandevuSistemi.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Core.Entities
{
    public class Appointment : BaseEntity
    {
        public int ServiceId { get; set; }
        public int EmployeeId { get; set; }
        public int CustomerId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime AppointmentEndTime { get; set; }
        public string Notes { get; set; }
        public AppointmentStatus Status { get; set; }
        public decimal Price { get; set; }
        public bool ReminderSent { get; set; }

        public virtual Service Service { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Customer Customer { get; set; }

    }
}
