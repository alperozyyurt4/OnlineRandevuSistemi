// OnlineRandevuSistemi.Business/BackgroundServices/AppointmentReminderService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Enums;
using OnlineRandevuSistemi.Core.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class AppointmentReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public AppointmentReminderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var appointmentRepository = scope.ServiceProvider.GetRequiredService<IRepository<Appointment>>();
                    var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();

                    var now = DateTime.Now;
                    var range = now.AddHours(24); // 24 saat içinde olanlar

                    var upcomingAppointments = await appointmentRepository.TableNoTracking
                        .Include(a => a.Customer).ThenInclude(c => c.User)
                        .Include(a => a.Employee).ThenInclude(e => e.User)
                        .Where(a =>
                            !a.IsDeleted &&
                            a.Status == AppointmentStatus.Confirmed &&
                            a.ReminderSent == false &&
                            a.AppointmentDate > now &&
                            a.AppointmentDate <= range)
                        .ToListAsync();

                    foreach (var appointment in upcomingAppointments)
                    {
                        await appointmentService.SendAppointmentReminderAsync(appointment.Id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ReminderService] Hata: {ex.Message}");
                }
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // 1 saatte bir tekrar et
        }
    }
}