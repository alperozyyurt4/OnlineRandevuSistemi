using AutoMapper;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Web.ViewModels;

namespace OnlineRandevuSistemi.Web.Mapping
{
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            CreateMap<DailySlotDto, DailySlotViewModel>().ReverseMap();
            CreateMap<TimeSlotDto, TimeSlotViewModel>().ReverseMap();

        }
    }
}
