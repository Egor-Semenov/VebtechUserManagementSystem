using Application.DTOs;
using AutoMapper;
using Domain.Models.Entities;

namespace Application.Mappers
{
    public sealed class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<User, UserDto>()
                .ForMember(x => x.Roles, opt => opt.MapFrom(x => x.Roles.Select(x => x.Role)));

            CreateMap<UserRegistrationDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
                .ForMember(x => x.Roles, opt => opt.Ignore());
        }
    }
}
