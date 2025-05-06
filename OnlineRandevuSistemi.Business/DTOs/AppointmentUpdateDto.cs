using OnlineRandevuSistemi.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.DTOs
{
    public class AppointmentUpdateDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int EmployeeId { get; set; }
        public int CustomerId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Notes { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}
