/*

using OnlineRandevuSistemi.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.Interfaces
{

    public interface IRedisCacheService
    {
        Task<IEnumerable<AppointmentDto>> GetOrSetAppointmentsAsync(Func<Task<IEnumerable<AppointmentDto>>> factory);
        Task<IEnumerable<CustomerDto>> GetOrSetCustomersAsync(Func<Task<IEnumerable<CustomerDto>>> factory);
        Task<IEnumerable<EmployeeDto>> GetOrSetEmployeesAsync(Func<Task<IEnumerable<EmployeeDto>>> factory);
        Task<IEnumerable<ServiceDto>> GetOrSetServicesAsync(Func<Task<IEnumerable<ServiceDto>>> factory);

        Task ClearCacheAsync(string key); // Cache silmek için
    }
}
*/