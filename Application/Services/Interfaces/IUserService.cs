using Application.DTOs;
using Application.Filters;
using Domain.Models.Entities;
using Domain.Models.Enums;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        public Task<List<UserDto>> GetUsersAsync(UserQueryFilterModel filterModel, int page, int pageSize);
        public Task<User> GetUserByIdAsync(int id);
        public Task<User> GetUserByEmailAsync(string email);
        public Task CreateUserAsync(UserRegistrationDto user);
        public Task UpdateUserAsync(int userId, UserUpdateDto user);
        public Task AddNewRoleToUserAsync(int userId, Roles newRole);
        public Task DeleteUserAsync(int userId);
    }
}
