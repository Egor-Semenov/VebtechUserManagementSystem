using Application.DTOs;
using Application.Services.Interfaces;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace VebtechTask.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IValidator<UserRegistrationDto> _userValidator;
        private readonly IValidator<UserAuthDto> _authValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService, 
            IUserService userService,
            IValidator<UserRegistrationDto> userValidator, 
            IValidator<UserAuthDto> authValidator,
            IMapper mapper,
            ILogger<AuthController> logger
            )
        {
            _authService = authService;
            _userService = userService;
            _userValidator = userValidator;
            _authValidator = authValidator;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = "Login user", Description = "Login a user in system")]
        [SwaggerResponse(StatusCodes.Status200OK, "User loged in system successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid email or password.")]
        public async Task<IActionResult> Authenticate([FromBody] UserAuthDto user)
        {
            await _authService.ValidateUserAsync(user);

            _logger.LogInformation($"User with {user.Email} email authorized");
            return Ok(new { Token = _authService.CreateToken(user.Email) });
        }
    }
}
