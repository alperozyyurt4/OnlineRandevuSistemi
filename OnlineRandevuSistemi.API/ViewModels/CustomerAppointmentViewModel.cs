using System;

namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class CustomerAppointmentViewModel
    {
        public string ServiceName { get; set; }

        public string EmployeeName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public string Notes { get; set; }
    }
}