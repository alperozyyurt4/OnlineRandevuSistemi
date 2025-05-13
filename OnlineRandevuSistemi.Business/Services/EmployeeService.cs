using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Enums;
using OnlineRandevuSistemi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityEmployeeService = OnlineRandevuSistemi.Core.Entities.EmployeeService;

namespace OnlineRandevuSistemi.Business.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<EntityEmployeeService> _employeeServiceRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<WorkingHour> _workingHourRepository;
        private readonly IRepository<Notification> _notificationRepository;
         private readonly IRedisCacheService _redisCacheService;

        public EmployeeService(
            IRepository<Employee> employeeRepository,
            IRepository<EntityEmployeeService> employeeServiceRepository,
            IRepository<Service> serviceRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IRepository<WorkingHour> workingHourRepository,
            IRepository<Notification> notificationRepository,
             IRedisCacheService redisCacheService

            )
        {
            _employeeRepository = employeeRepository;
            _employeeServiceRepository = employeeServiceRepository;
            _serviceRepository = serviceRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workingHourRepository = workingHourRepository;
            _notificationRepository = notificationRepository;
            _redisCacheService = redisCacheService;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            return await _redisCacheService.GetOrSetEmployeesAsync(async () =>
            {
                var employees = await _employeeRepository.TableNoTracking
              .Include(e => e.User)
              .Where(e => !e.IsDeleted)
              .Include(e => e.EmployeeServices)
              .ToListAsync();

                return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            });
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(int id)
        {
            var employee = await _employeeRepository.TableNoTracking
                .Include(e => e.User)
                .Include(e => e.EmployeeServices)
                .ThenInclude(es => es.Service)
                .Include(e => e.WorkingHours)
                .FirstOrDefaultAsync(e => e.Id == id);
            var dto = _mapper.Map<EmployeeDto>(employee);
            dto.ServiceIds = employee.EmployeeServices.Select(es => es.ServiceId).ToList();
            return dto;
        }

        public async Task<EmployeeDto> GetEmployeeByUserIdAsync(string userId)
        {
            var employee = await _employeeRepository.TableNoTracking
                .Include(e => e.User)
                .Include(e => e.EmployeeServices)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByServiceIdAsync(int serviceId)
        {
            var employees = await _employeeRepository.TableNoTracking
                .Include(e => e.User)
                .Include(e => e.EmployeeServices)
                .Where(e => e.EmployeeServices.Any(es => es.ServiceId == serviceId))
                .ToListAsync();

            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }
        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto)
        {
            var employee = _mapper.Map<Employee>(employeeDto);
            await _employeeRepository.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            // 🔥 Yeni: Hizmet Atamaları
            if (employeeDto.ServiceIds != null && employeeDto.ServiceIds.Any())
            {
                foreach (var serviceId in employeeDto.ServiceIds)
                {
                    var employeeService = new EntityEmployeeService
                    {
                        EmployeeId = employee.Id,
                        ServiceId = serviceId,
                        CreatedDate = DateTime.Now
                    };

                    await _employeeServiceRepository.AddAsync(employeeService);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            // 🕓 Varsayılan Çalışma Saatleri
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) continue;

                var workingHour = new WorkingHour
                {
                    EmployeeId = employee.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    IsWorkingDay = true,
                    CreatedDate = DateTime.Now,
                };

                await _workingHourRepository.AddAsync(workingHour);
            }

            await _unitOfWork.SaveChangesAsync();
            await _redisCacheService.ClearCacheAsync("employees-all");


            return _mapper.Map<EmployeeDto>(employee);
        }
        public async Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto employeeDto)
        {
            var employee = await _employeeRepository.Table
                .Include(e => e.User)
                .Include(e => e.EmployeeServices)
                .FirstOrDefaultAsync(e => e.Id == employeeDto.Id);

            if (employee == null)
                throw new Exception("Employee not found");

            // AppUser güncellemesi
            employee.User.FirstName = employeeDto.FirstName;
            employee.User.LastName = employeeDto.LastName;
            employee.User.Email = employeeDto.Email;
            employee.User.UserName = employeeDto.Email;
            employee.User.PhoneNumber = employeeDto.PhoneNumber;
            employee.User.Address = employeeDto.Address;
            employee.User.DateOfBirth = employeeDto.DateOfBirth ?? DateTime.Now;
            employee.User.ProfilePicture = employeeDto.ProfilePicture ?? "/images/default-profile.jpg";
            employee.User.UpdatedDate = DateTime.Now;

            // Employee güncellemesi
            employee.Position = employeeDto.Position;
            employee.Biography = employeeDto.Biography;
            employee.HourlyDate = employeeDto.HourlyDate;
            employee.IsAvailable = employeeDto.IsAvailable;
            employee.UpdatedDate = DateTime.Now;

            // 🔥 Hizmet eşleştirmelerini sıfırla ve yeniden ata
            var existingServices = await _employeeServiceRepository
                .Table
                .Where(es => es.EmployeeId == employee.Id)
                .ToListAsync();

            foreach (var item in existingServices)
            {
                await _employeeServiceRepository.DeleteAsync(item.Id);
            }
            employee.EmployeeServices.Clear();

            if (employeeDto.ServiceIds != null && employeeDto.ServiceIds.Any())
            {
                foreach (var serviceId in employeeDto.ServiceIds)
                {
                    var employeeService = new EntityEmployeeService
                    {
                        EmployeeId = employee.Id,
                        ServiceId = serviceId,
                        CreatedDate = DateTime.Now
                    };
                    await _employeeServiceRepository.AddAsync(employeeService);
                }
            }

            await _employeeRepository.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();
            await _redisCacheService.ClearCacheAsync("employees-all");


            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _employeeRepository.Table
                .Include(a => a.User)
                .Include(a => a.Appointments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return false;

            employee.IsDeleted = true;
            employee.UpdatedDate = DateTime.Now;
            if (employee.User != null)
            {
                employee.User.IsDeleted = true;
                employee.User.UpdatedDate = DateTime.Now;
            }  
            foreach (var appointment in employee.Appointments)
            {
                appointment.IsDeleted = true;
                appointment.UpdatedDate = DateTime.Now;
            }


            await _employeeRepository.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();
            
            await _redisCacheService.ClearCacheAsync("employees-all");


            return true;
        }

        public async Task<bool> AddServiceToEmployeeAsync(int employeeId, int serviceId)
        {
            var exists = await _employeeServiceRepository.TableNoTracking
                .AnyAsync(es => es.EmployeeId == employeeId && es.ServiceId == serviceId);

            if (exists)
                return false;

            var relation = new EntityEmployeeService
            {
                EmployeeId = employeeId,
                ServiceId = serviceId,
                CreatedDate = DateTime.Now
            };

            await _employeeServiceRepository.AddAsync(relation);
            await _unitOfWork.SaveChangesAsync();

            var employee = await _employeeRepository.TableNoTracking
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == employeeId);
            var service = await _serviceRepository.GetByIdAsync(serviceId);

            if (employee != null && service != null)
            {
                await _notificationRepository.AddAsync(new Notification()
                {
                    UserId = employee.User.Id,
                    Title = "Yeni Hizmet Ataması",
                    Message = $"Size \"{service.Name}\" hizmeti atanmıştır.",
                    Type = NotificationType.System,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                await _unitOfWork.SaveChangesAsync();

            }

            return true;
        }

        public async Task<bool> RemoveServiceFromEmployeeAsync(int employeeId, int serviceId)
        {
            var relation = await _employeeServiceRepository.Table
                .FirstOrDefaultAsync(es => es.EmployeeId == employeeId && es.ServiceId == serviceId);

            if (relation == null)
                return false;

            await _employeeServiceRepository.DeleteAsync(relation.Id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
