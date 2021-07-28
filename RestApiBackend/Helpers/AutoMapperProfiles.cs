using AutoMapper;
using RestApiBackend.Contracts.V1.Responses;
using RestApiBackend.Entities;

namespace RestApiBackend.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserDtoResponse>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles));
            CreateMap<Role, RoleDtoResponse>();
            CreateMap<UserRole, RoleDtoResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Role.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Role.Name));
        }
    }
}