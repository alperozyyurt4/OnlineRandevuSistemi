using OnlineRandevuSistemi.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.Interfaces
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceDto>> GetAllServicesAsync();
        Task<ServiceDto> GetServiceByIdAsync(int id);
        Task<ServiceDto> CreateServiceAsync(ServiceDto serviceDto);
        Task<ServiceDto> UpdateServiceAsync(ServiceDto serviceDto);
        Task<bool> DeleteServiceAsync(int id);

    }
}
