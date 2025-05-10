// OnlineRandevuSistemi.Business/Interfaces/IAppointmentService.cs
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto> GetAppointmentByIdAsync(int id);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerIdAsync(int customerId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<AppointmentDto> CreateAppointmentAsync(AppointmentCreateDto appointmentDto);
        Task<AppointmentDto> UpdateAppointmentAsync(AppointmentUpdateDto appointmentDto);
        Task<bool> DeleteAppointmentAsync(int id);
        Task<bool> UpdateAppointmentStatusAsync(int id, AppointmentStatus status);
        Task<bool> CheckEmployeeAvailabilityAsync(int employeeId, DateTime appointmentDate, int durationMinutes);
        Task SendAppointmentReminderAsync(int appointmentId);
        Task<List<DailySlotDto>> GetWeeklyAvailabilityAsync(int employeeId, DateTime startDate, DateTime endDate);
        Task<DailySlotDto> GetDailyAvailabilityAsync(int employeeId, DateTime date);
    }
}