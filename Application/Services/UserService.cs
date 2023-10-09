using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class UserService : IUserService
    {
        private readonly IBaseRepository<User> _userRepository;

        public UserService(IBaseRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public Task CreateUserAsync(User user) =>
            _userRepository.Create(user);

        public Task DeleteUserAsync(User user) =>
            _userRepository.Delete(user);

        public Task<User> GetUserByIdAsync(int id) =>
            _userRepository.FindByCondition(x => x.Id == id).FirstOrDefaultAsync();

        public Task<User> GetUserByEmailAsync(string email) =>
            _userRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();

        public Task<List<User>> GetUsersAsync() =>
            _userRepository.FindAll().ToListAsync();

        public Task UpdateUserAsync(User user) =>
            _userRepository.Update(user);
    }
}
