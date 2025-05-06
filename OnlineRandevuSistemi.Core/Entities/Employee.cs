using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Core.Entities
{
    public class Employee : BaseEntity
    {
        public string UserId { get; set; }
        public string Position { get; set; }
        public string Biography { get; set; }
        public decimal HourlyDate { get; set; }
        public bool IsAvailable { get; set; }

        public virtual AppUser User { get; set; }
        public virtual ICollection<EmployeeService> EmployeeServices { get; set; }
        public virtual ICollection<WorkingHour> WorkingHours { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}
