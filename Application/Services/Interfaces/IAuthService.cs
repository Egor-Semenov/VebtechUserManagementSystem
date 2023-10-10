using Application.DTOs;

namespace Application.Services.Interfaces
{
    public interface IAuthService
    {
        public Task ValidateUserAsync(UserAuthDto userAuthDto);
        public string CreateToken(string email);
    }
}
