// AppointmentListViewModel.cs
using OnlineRandevuSistemi.Business.DTOs;
using System.Collections.Generic;

namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class AppointmentListViewModel
    {
        public List<AppointmentDto> UpcomingAppointments { get; set; } = new();
        public List<AppointmentDto> PastAppointments { get; set; } = new();
    }
}