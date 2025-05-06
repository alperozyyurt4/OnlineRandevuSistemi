using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.DataAccess.Context
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { 
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<EmployeeService> EmployeeServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<WorkingHour> WorkingHours { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Decimal hassasiyet ayarları
            builder.Entity<Service>()
                   .Property(s => s.Price)
                   .HasPrecision(18, 2);

            builder.Entity<Appointment>()
                   .Property(a => a.Price)
                   .HasPrecision(18, 2);

            builder.Entity<Employee>()
                   .Property(e => e.HourlyDate)
                   .HasPrecision(18, 2);

            builder.Entity<EmployeeService>()
                   .Property(es => es.CustomPrice)
                   .HasPrecision(18, 2);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
