using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Core.Entities
{
    public class Service : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string ImageUrl { get; set; }


        public virtual ICollection<EmployeeService> EmployeeServices { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}
