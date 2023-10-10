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
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(x => x.Roles.Select(x => x.Role)));

            CreateMap<UserRegistrationDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(x => BCrypt.Net.BCrypt.HashPassword(x.Password)))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(x => x.Email.ToLower()))
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
        }
    }
}
