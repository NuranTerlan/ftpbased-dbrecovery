using AutoMapper;
using FTPBasedSystem.DOMAINENTITIES.DTOs;
using FTPBasedSystem.DOMAINENTITIES.Models;

namespace FTPBasedSystem.SERVICES.MappingProfiles
{
    public class EntitiesMappingProfile : Profile
    {
        public EntitiesMappingProfile()
        {
            CreateMap<TextDto, Text>().ReverseMap();
            CreateMap<NumberDto, Number>().ReverseMap();
            CreateMap<DateDto, Date>().ReverseMap();
        }
    }
}