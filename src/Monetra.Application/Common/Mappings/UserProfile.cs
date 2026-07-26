using AutoMapper;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Entities;

namespace Monetra.Application.Common.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Value))
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.IsPremium, opt => opt.MapFrom(s => s.IsPremium));

        CreateMap<Person, PersonDto>();
    }
}
