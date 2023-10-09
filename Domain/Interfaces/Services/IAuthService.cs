using Domain.Models.Entities;

namespace Domain.Interfaces.Services
{
    public interface IAuthService
    {
        public Task CreateUserAsync(User user);
        public Task<bool> ValidateUser(string email, string password);
        public Task<string> CreateToken(string email);
    }
}
