using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
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

        public EmployeeService(
            IRepository<Employee> employeeRepository,
            IRepository<EntityEmployeeService> employeeServiceRepository,
            IRepository<Service> serviceRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _employeeServiceRepository = employeeServiceRepository;
            _serviceRepository = serviceRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepository.TableNoTracking
                .Include(e => e.User)
                .Include(e => e.EmployeeServices)
                .ToListAsync();

            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(int id)
        {
            var employee = await _employeeRepository.TableNoTracking
                .Include(e => e.User)
                .Include(e => e.EmployeeServices)
                .FirstOrDefaultAsync(e => e.Id == id);

            return _mapper.Map<EmployeeDto>(employee);
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
            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto employeeDto)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeDto.Id);
            if (employee == null)
                throw new Exception("Employee not found");

            employee = _mapper.Map(employeeDto, employee);
            employee.UpdatedDate = DateTime.Now;
            await _employeeRepository.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                return false;

            await _employeeRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
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
