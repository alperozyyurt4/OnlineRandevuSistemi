﻿using OnlineRandevuSistemi.Business.DTOs;

namespace OnlineRandevuSistemi.Web.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalServices { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public List<PopularServiceViewModel> PopularServices { get; set; }
    }
}