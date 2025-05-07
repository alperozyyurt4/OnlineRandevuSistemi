// OnlineRandevuSistemi.Business/Mapping/MappingProfile.cs
using AutoMapper;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Core.Entities;

namespace OnlineRandevuSistemi.Business.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<AppUser, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
            CreateMap<UserDto, AppUser>();

            // Appointment
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.User.FirstName + " " + src.Employee.User.LastName))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.User.FirstName + " " + src.Customer.User.LastName));

            CreateMap<AppointmentCreateDto, Appointment>()
                .ForMember(dest => dest.AppointmentEndTime, opt => opt.Ignore());

            CreateMap<AppointmentUpdateDto, Appointment>()
                .ForMember(dest => dest.AppointmentEndTime, opt => opt.Ignore());

            // Service
            CreateMap<Service, ServiceDto>().ReverseMap();

            // Employee
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
            CreateMap<EmployeeDto, Employee>();

            // Customer
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
            CreateMap<CustomerDto, Customer>();
        }
    }
}
