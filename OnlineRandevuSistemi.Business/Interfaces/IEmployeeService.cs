using OnlineRandevuSistemi.Business.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto> GetEmployeeByUserIdAsync(string userId);
        Task<IEnumerable<EmployeeDto>> GetEmployeesByServiceIdAsync(int serviceId);
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto);
        Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto employeeDto);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<bool> AddServiceToEmployeeAsync(int employeeId, int serviceId);
        Task<bool> RemoveServiceFromEmployeeAsync(int employeeId, int serviceId);
    }
}
