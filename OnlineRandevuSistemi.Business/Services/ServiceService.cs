// OnlineRandevuSistemi.Business/Services/ServiceService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IRepository<Service> _serviceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmployeeService _employeeService;
        private readonly IMemoryCache _cache;

        public ServiceService(
            IRepository<Service> serviceRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmployeeService employeeService,
            IMemoryCache cache
            
            )

        {
            _serviceRepository = serviceRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _employeeService = employeeService;
            _cache = cache;
        }

        public async Task<IEnumerable<ServiceDto>> GetAllServicesAsync()
        {
            var services = await _serviceRepository.TableNoTracking
                .Where(s => !s.IsDeleted)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ServiceDto>>(services);
        }

        public async Task<ServiceDto> GetServiceByIdAsync(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            return _mapper.Map<ServiceDto>(service);
        }

        public async Task<ServiceDto> CreateServiceAsync(ServiceDto serviceDto)
        {
            var service = _mapper.Map<Service>(serviceDto);
            await _serviceRepository.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ServiceDto>(service);
        }

        public async Task<ServiceDto> UpdateServiceAsync(ServiceDto serviceDto)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceDto.Id);
            if (service == null)
                throw new Exception("Service not found");

            service = _mapper.Map(serviceDto, service);
            service.UpdatedDate = DateTime.Now;

            await _serviceRepository.UpdateAsync(service);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ServiceDto>(service);
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var service = await _serviceRepository.Table
                .Include(s => s.Appointments)
                .Include(s => s.EmployeeServices)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return false;

            foreach (var app in service.Appointments)
            {
                app.IsDeleted = true;
                app.UpdatedDate = DateTime.Now; 
            }
            foreach (var es in service.EmployeeServices)
            {
                await _employeeService.DeleteEmployeeAsync(es.Id);
            }
            service.IsDeleted = true;
            service.UpdatedDate = DateTime.Now;

            await _serviceRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        public async Task<List<ServiceDto>> GetPopularServicesAsync()
        {
            if (!_cache.TryGetValue("popular_services", out List<ServiceDto> cachedServices))
            {
                var services = await _serviceRepository.TableNoTracking
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.CreatedDate)
                    .Take(5)
                    .ToListAsync();

                cachedServices = _mapper.Map<List<ServiceDto>>(services);

                _cache.Set("popular_services", cachedServices,
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    });
            }

            return cachedServices;
        }

    }
}