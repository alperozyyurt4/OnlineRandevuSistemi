// OnlineRandevuSistemi.Business/Services/AppointmentService.cs
using AutoMapper;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Enums;
using OnlineRandevuSistemi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OnlineRandevuSistemi.Business.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AppointmentService(
            IRepository<Appointment> appointmentRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Service> serviceRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _employeeRepository = employeeRepository;
            _serviceRepository = serviceRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            var appointments = await _appointmentRepository.TableNoTracking
                .Where(a => !a.IsDeleted)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                .Include(a => a.Customer)
                    .ThenInclude(c => c.User)
                .Include(a => a.Service)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }


        public async Task<AppointmentDto> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.TableNoTracking
                .Where(a => !a.IsDeleted)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                .Include(a => a.Customer)
                    .ThenInclude(c => c.User)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            return _mapper.Map<AppointmentDto>(appointment);
        }


        // OnlineRandevuSistemi.Business/Services/AppointmentService.cs - Kalan metotlar
        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            var appointments = await _appointmentRepository.TableNoTracking
                
                .Where(a => a.CustomerId == customerId && !a.IsDeleted)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                .Include(a => a.Customer)
                    .ThenInclude(c => c.User)
                .Include(a => a.Service)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByEmployeeIdAsync(int employeeId)
        {
            var appointments = await _appointmentRepository.TableNoTracking
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .Include(a => a.Customer)
                .Where(a => a.EmployeeId == employeeId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var appointments = await _appointmentRepository.TableNoTracking
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .Include(a => a.Customer)
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(AppointmentCreateDto appointmentDto)
        {
            var service = await _serviceRepository.GetByIdAsync(appointmentDto.ServiceId);
            if (service == null)
                throw new Exception("Service not found");

            // Calculate end time based on service duration
            var appointment = _mapper.Map<Appointment>(appointmentDto);
            appointment.AppointmentEndTime = appointment.AppointmentDate.AddMinutes(service.DurationMinutes);
            appointment.Price = service.Price;

            await _appointmentRepository.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task<AppointmentDto> UpdateAppointmentAsync(AppointmentUpdateDto appointmentDto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentDto.Id);
            if (appointment == null)
                throw new Exception("Appointment not found");

            var service = await _serviceRepository.GetByIdAsync(appointmentDto.ServiceId);
            if (service == null)
                throw new Exception("Service not found");

            appointment = _mapper.Map(appointmentDto, appointment);
            appointment.AppointmentEndTime = appointment.AppointmentDate.AddMinutes(service.DurationMinutes);
            appointment.UpdatedDate = DateTime.Now;

            await _appointmentRepository.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return false;

            appointment.IsDeleted = true;
            appointment.UpdatedDate = DateTime.Now;

            await _appointmentRepository.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int id, AppointmentStatus status)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return false;

            appointment.Status = status;
            appointment.UpdatedDate = DateTime.Now;

            await _appointmentRepository.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CheckEmployeeAvailabilityAsync(int employeeId, DateTime appointmentDate, int durationMinutes)
        {
            var appointmentEndTime = appointmentDate.AddMinutes(durationMinutes);

            // Check if employee has any overlapping appointments
            var hasOverlappingAppointments = await _appointmentRepository.TableNoTracking
                .Where(a => a.EmployeeId == employeeId && !a.IsDeleted && a.Status != AppointmentStatus.Cancelled)
                .AnyAsync(a =>
                    (appointmentDate >= a.AppointmentDate && appointmentDate < a.AppointmentEndTime) ||
                    (appointmentEndTime > a.AppointmentDate && appointmentEndTime <= a.AppointmentEndTime) ||
                    (appointmentDate <= a.AppointmentDate && appointmentEndTime >= a.AppointmentEndTime));

            if (hasOverlappingAppointments)
                return false;

            // Check if the time is within employee's working hours
            var dayOfWeek = appointmentDate.DayOfWeek;
            var timeOfDay = appointmentDate.TimeOfDay;

            var employee = await _employeeRepository.TableNoTracking
                .Include(e => e.WorkingHours)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                return false;

            var workingHour = employee.WorkingHours
                .FirstOrDefault(wh => wh.DayOfWeek == dayOfWeek && wh.IsWorkingDay);

            if (workingHour == null)
                return false;

            return timeOfDay >= workingHour.StartTime &&
                   appointmentEndTime.TimeOfDay <= workingHour.EndTime;
        }

        public async Task SendAppointmentReminderAsync(int appointmentId)
        {
            var appointment = await _appointmentRepository.TableNoTracking
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || appointment.ReminderSent)
                return;

            // Send reminder logic (e-mail, SMS, etc.) would be implemented here
            // For now, just mark as sent
            appointment.ReminderSent = true;
            await _appointmentRepository.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}