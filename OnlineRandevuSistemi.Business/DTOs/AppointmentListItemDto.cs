using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.DTOs
{
    public class AppointmentListItemDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string EmployeeFullName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
    }
}
