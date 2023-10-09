using Application.DTOs;
using AutoMapper;
using Domain.Interfaces.Services;
using Domain.Models.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;

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

        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Register new user", Description = "Register a new user.")]
        [SwaggerResponse(StatusCodes.Status201Created, "New user registered successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or user with the same email already exists")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto userRegistrationDto)
        {
            var validationResult = _userValidator.Validate(userRegistrationDto);

            if (!validationResult.IsValid)
            {
                var stringBuilder = new StringBuilder();
                foreach (var error in validationResult.Errors)
                {
                    stringBuilder.AppendLine(error.ErrorMessage);
                }

                _logger.LogError($"Invalid registration request: {Environment.NewLine} {stringBuilder}");
                return BadRequest(stringBuilder.ToString());
            }

            var userByEmailInDto = await _userService.GetUserByEmailAsync(userRegistrationDto.Email);
            if (userByEmailInDto is not null)
            {
                _logger.LogError($"Invalid request: user with {userRegistrationDto.Email} already exists");
                return BadRequest("User with the same email already exists.");
            }

            var user = _mapper.Map<User>(userRegistrationDto);

            foreach (var role in userRegistrationDto.Roles)
            {
                user.Roles.Add(new UserRoles
                {
                    Role = role,
                    UserId = user.Id
                });
            };

            await _authService.CreateUserAsync(user);

            _logger.LogInformation($"User created");
            return StatusCode(201);
        }

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = "Login user", Description = "Login a user in system")]
        [SwaggerResponse(StatusCodes.Status200OK, "User loged in system successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid email or password.")]
        public async Task<IActionResult> Authenticate([FromBody] UserAuthDto user)
        {
            var validationResult = _authValidator.Validate(user);
            if (!validationResult.IsValid)
            {
                var stringBuilder = new StringBuilder();
                foreach (var error in validationResult.Errors)
                {
                    stringBuilder.AppendLine(error.ErrorMessage);
                }

                _logger.LogError($"Invalid auth request: {Environment.NewLine} {stringBuilder}");
                return BadRequest(stringBuilder.ToString());
            }

            if (await _authService.ValidateUser(user.Email, user.Password))
            {
                _logger.LogError("Auth failed");
                return Unauthorized("Invalid email or password");
            }

            _logger.LogInformation($"User with {user.Email} email authorized");
            return Ok(new { Token = await _authService.CreateToken(user.Email) });
        }
    }
}
