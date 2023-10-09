using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IBaseRepository<User> _userRepository;

        public AuthService(IConfiguration configuration, IBaseRepository<User> userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public Task CreateUserAsync(User user) =>
            _userRepository.Create(user);

        public async Task<bool> ValidateUser(string email, string password)
        {
            var user = await _userRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();

            return user == null || !ValidatePassword(password, user.PasswordHash);
        }

        public Task<string> CreateToken(string email) => Task.FromResult(GenerateJwtToken(email));

        private string GenerateJwtToken(string email)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSettings").GetSection("SECRET").Value!);
            var secret = new SymmetricSecurityKey(key);

            var signingCredentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
            };

            var tokenOptions = new JwtSecurityToken
                (
                    issuer: jwtSettings.GetSection("validIssuer").Value,
                    audience: jwtSettings.GetSection("validAudience").Value,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("expires").Value)),
                    signingCredentials: signingCredentials
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        private bool ValidatePassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
