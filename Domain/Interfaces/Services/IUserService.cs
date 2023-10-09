using Domain.Models.Entities;

namespace Domain.Interfaces.Services
{
    public interface IUserService
    {
        public Task<List<User>> GetUsersAsync();
        public Task<User> GetUserByIdAsync(int id);
        public Task<User> GetUserByEmailAsync(string email);
        public Task CreateUserAsync(User user);
        public Task UpdateUserAsync(User user);
        public Task DeleteUserAsync(User user);
    }
}
