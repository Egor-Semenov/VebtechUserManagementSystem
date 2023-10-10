using Application.DTOs;
using Application.Middleware;
using Application.Services.Interfaces;
using FluentValidation;
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
        private readonly IUserService _userService;
        private readonly IValidator<UserAuthDto> _authValidator;

        public AuthService(
            IConfiguration configuration, 
            IUserService userService, 
            IValidator<UserAuthDto> authValidator
            )
        {
            _configuration = configuration;
            _userService = userService;
            _authValidator = authValidator;
        }

        public async Task ValidateUserAsync(UserAuthDto userAuthDto)
        {
            var validationResult = _authValidator.Validate(userAuthDto);
            if (!validationResult.IsValid)
            {
                var stringBuilder = new StringBuilder();
                foreach (var error in validationResult.Errors)
                {
                    stringBuilder.AppendLine(error.ErrorMessage);
                }

                throw new BadRequestException(stringBuilder.ToString().Trim());
            }

            var user = await _userService.GetUserByEmailAsync(userAuthDto.Email);
            if (user == null || !ValidatePassword(userAuthDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password");
            }
        }

        public string CreateToken(string email) => GenerateJwtToken(email);

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
