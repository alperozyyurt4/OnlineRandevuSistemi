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
using Microsoft.AspNetCore.Mvc;

namespace OnlineRandevuSistemi.Business.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<Customer> _customerRepository;
//        private readonly IRedisCacheService _redisCacheService;

        public AppointmentService(
            IRepository<Appointment> appointmentRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Service> serviceRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ISmsService smsService,
            IEmailService emailService,
            IRepository<Notification> notificationRepository,
            IRepository<Customer> customerRepository


//          IRedisCacheService redisCacheService
)
        {
            _appointmentRepository = appointmentRepository;
            _employeeRepository = employeeRepository;
            _serviceRepository = serviceRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _smsService = smsService;
            _emailService = emailService;
            _notificationRepository = notificationRepository;
            _customerRepository = customerRepository;
            //            _redisCacheService = redisCacheService;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
//          return await _redisCacheService.GetOrSetAppointmentsAsync(async () =>
//          {
                var appointments = await _appointmentRepository.TableNoTracking
                .Where(a => !a.IsDeleted)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                .Include(a => a.Customer)
                    .ThenInclude(c => c.User)
                .Include(a => a.Service)
                .ToListAsync();

                return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
//            });
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
                .Where(a => a.EmployeeId == employeeId && !a.IsDeleted)
                .Include(a => a.Employee)
                    .ThenInclude (e => e.User)
                .Include(a => a.Service)
                .Include(a => a.Customer)
                    .ThenInclude(c => c.User)
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

            // Appointment oluştur
            var appointment = _mapper.Map<Appointment>(appointmentDto);
            appointment.AppointmentEndTime = appointment.AppointmentDate.AddMinutes(service.DurationMinutes);
            appointment.Price = service.Price;

            await _appointmentRepository.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync(); // ID oluşsun diye önce kaydediyoruz

            // Customer → AppUser ilişkisinden UserId al
            var customer = await _customerRepository.TableNoTracking
                .FirstOrDefaultAsync(c => c.Id == appointment.CustomerId);

            if (customer != null)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = customer.UserId,
                    AppointmentId = appointment.Id,
                    Title = "Randevu Oluşturuldu",
                    Message = $"{appointment.AppointmentDate:dd.MM.yyyy HH:mm} tarihli randevunuz başarıyla oluşturuldu.",
                    Type = NotificationType.Appointment,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });

                await _unitOfWork.SaveChangesAsync(); // 🔥 Notification'ı kaydet
            }
            var employee = await _employeeRepository.TableNoTracking
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == appointment.EmployeeId);

            if (employee != null)
            {
                await _notificationRepository.AddAsync(new Notification()
                {
                    UserId = employee.User.Id,
                    AppointmentId = appointment.Id,
                    Title = "Yeni bir randevu atandı",
                    Message = $"{appointment.AppointmentDate:dd:MM:yyyy HH:mm} tarihli bir randevu size atandı",
                    Type = NotificationType.Appointment,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                await _unitOfWork.SaveChangesAsync(); // 🔥 Notification'ı kaydet

            }


            //          await _redisCacheService.ClearCacheAsync("appointments-all");


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

            var customer = await _customerRepository.TableNoTracking
                .FirstOrDefaultAsync(c => c.Id == appointment.CustomerId);

            var employee = await _employeeRepository.TableNoTracking
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == appointment.EmployeeId);

            if (customer != null)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = customer.UserId,
                    AppointmentId = appointment.Id,
                    Title = "Randevu Detayları Güncellendi",
                    Message = $"{appointment.AppointmentDate:dd.MM.yyyy HH:mm} tarihli randevunuz güncellendi.",
                    Type = NotificationType.Appointment,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });

                await _unitOfWork.SaveChangesAsync(); // 🔥 Notification'ı kaydet
            }

            if (employee != null)
            {
                await _notificationRepository.AddAsync(new Notification()
                {
                    UserId = employee.User.Id,
                    AppointmentId = appointment.Id,
                    Title = "Randevu Detayları Güncellendi",
                    Message = $"{appointment.AppointmentDate:dd:MM:yyyy HH:mm} tarihli bir randevunuz güncellendi.",
                    Type = NotificationType.Appointment,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                await _unitOfWork.SaveChangesAsync(); // 🔥 Notification'ı kaydet

            }


            //          await _redisCacheService.ClearCacheAsync("appointments-all");

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

            // Bildirim ekle (iptal bildirimi)
            var customer = await _customerRepository.TableNoTracking
                .FirstOrDefaultAsync(c => c.Id == appointment.CustomerId);
            var employee = await _employeeRepository.TableNoTracking
             .Include(e => e.User)
             .FirstOrDefaultAsync(e => e.Id == appointment.EmployeeId);


            if (customer != null)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = customer.UserId,
                    AppointmentId = appointment.Id,
                    Title = "Randevu İptal Edildi",
                    Message = $"{appointment.AppointmentDate:dd.MM.yyyy HH:mm} tarihli randevunuz iptal edilmiştir.",
                    Type = NotificationType.Cancellation,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });

                await _unitOfWork.SaveChangesAsync(); // Bildirimi kaydet
            }
            if (employee != null)
            {
                await _notificationRepository.AddAsync(new Notification()
                {
                    UserId = employee.User.Id,
                    AppointmentId = appointment.Id,
                    Title = "Randevu İptal Edildi",
                    Message = $"{appointment.AppointmentDate:dd:MM:yyyy HH:mm} tarihli randevunuz iptal edildi.",
                    Type = NotificationType.Appointment,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                await _unitOfWork.SaveChangesAsync(); // 🔥 Notification'ı kaydet

            }


            //          await _redisCacheService.ClearCacheAsync("appointments-all");


            return true;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int id, AppointmentStatus status)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return false;

            if ((status == AppointmentStatus.Completed || status == AppointmentStatus.NoShow) && appointment.AppointmentDate > DateTime.Now)
            {
                throw new Exception("Ramdevu tarihi henüz gelmediği için bu durumu seçemezsiniz.");
            }
            if ((status == AppointmentStatus.Pending || status == AppointmentStatus.Confirmed) && appointment.AppointmentDate < DateTime.Now)
            {
                throw new Exception("Geçmiş bir randevuya bu durum atanamaz.");
            }



            appointment.Status = status;
            appointment.UpdatedDate = DateTime.Now;

            await _appointmentRepository.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            var customer = await _customerRepository.TableNoTracking
              .FirstOrDefaultAsync(c => c.Id == appointment.CustomerId);

            var employee = await _employeeRepository.TableNoTracking
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == appointment.EmployeeId);

            if (customer != null)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = customer.UserId,
                    AppointmentId = appointment.Id,
                    Title = "Randevu Durumu Güncellendi",
                    Message = $"{appointment.AppointmentDate:dd.MM.yyyy HH:mm} tarihli randevunuzun durumu güncellendi.",
                    Type = NotificationType.Appointment,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });

                await _unitOfWork.SaveChangesAsync(); // 🔥 Notification'ı kaydet
            }

            if (employee != null)
            {
                await _notificationRepository.AddAsync(new Notification()
                {
                    UserId = employee.User.Id,
                    AppointmentId = appointment.Id,
                    Title = "Randevu Durumu Güncellendi",
                    Message = $"{appointment.AppointmentDate:dd:MM:yyyy HH:mm} tarihli randevunuzun durumu güncellendi.",
                    Type = NotificationType.Appointment,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                await _unitOfWork.SaveChangesAsync(); // 🔥 Notification'ı kaydet

            }

            //          await _redisCacheService.ClearCacheAsync("appointments-all");

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
            /*
            if (appointment == null || appointment.ReminderSent)
                return;
            */

            if (appointment == null)
                return;

            await _emailService.SendAsync(
                appointment.Customer.User.Email,
                "Randevu Hatırlatma",
                $"{appointment.AppointmentDate:dd-MM-yyyy HH:mm} tarihli randevunuz yaklaşıyor"
             );

            await _smsService.SendAsync(
                appointment.Customer.User.PhoneNumber,
                "Yaklaşan randevunuz var unutmayın"
                );
            // Send reminder logic (e-mail, SMS, etc.) would be implemented here
            // For now, just mark as sent
            appointment.ReminderSent = true;
            await _appointmentRepository.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<List<DailySlotDto>> GetWeeklyAvailabilityAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            var employee = await _employeeRepository.TableNoTracking
                .Include(e => e.WorkingHours)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            var appointments = await _appointmentRepository.TableNoTracking
                .Where(a => a.EmployeeId == employeeId && a.AppointmentDate.Date >= startDate && a.AppointmentDate.Date <= endDate)
                .ToListAsync();

            var result = new List<DailySlotDto>();

            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var wh = employee.WorkingHours.FirstOrDefault(w => w.DayOfWeek == day.DayOfWeek && w.IsWorkingDay);
                if (wh == null) continue;

                var slots = new List<TimeSlotDto>();
                for (var time = wh.StartTime; time < wh.EndTime; time += TimeSpan.FromMinutes(30))
                {
                    var dateTime = day.Date + time;
                    bool isBusy = appointments.Any(a =>
                        dateTime >= a.AppointmentDate && dateTime < a.AppointmentEndTime);

                    slots.Add(new TimeSlotDto
                    {
                        Time = time,
                        IsAvailable = !isBusy
                    });
                }

                result.Add(new DailySlotDto
                {
                    Date = day,
                    TimeSlots = slots
                });
            }

            return result;
        }
        public async Task<DailySlotDto> GetDailyAvailabilityAsync(int employeeId, DateTime date)
        {
            var weekly = await GetWeeklyAvailabilityAsync(employeeId, date, date);
            return weekly.FirstOrDefault(); // Sadece tek bir gün getiriyoruz
        }
    }
}