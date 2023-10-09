using Application.DTOs;
using AutoMapper;
using Domain.Interfaces.Services;
using Domain.Models.Entities;
using Domain.Models.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VebtechTask.Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public sealed class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<UserRegistrationDto> _userValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        private readonly HashSet<string> _orderByFields = new HashSet<string>
        {
            "id",
            "name",
            "age",
            "email"
        };

        public UserController(
            IUserService userService, 
            IValidator<UserRegistrationDto> userValidator, 
            IMapper mapper,
            ILogger<UserController> logger
            )
        {
            _userService = userService;
            _userValidator = userValidator;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Get all users", Description = "Retrieves a list of all users.")]
        [SwaggerResponse(StatusCodes.Status200OK, "List of users retrieved successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request.")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string orderBy = "Id",
            [FromQuery] string filter = ""
            )
        {
            if (!_orderByFields.Contains(orderBy.ToLower()))
            {
                _logger.LogError($"Invalid order field: {orderBy}");
                return BadRequest($"Can't order by {orderBy} field.");
            }

            var users =  await _userService.GetUsersAsync();

            if (!string.IsNullOrEmpty(filter))
            {
                users = users.Where(x =>
                    x.Name.Contains(filter) ||
                    x.Email.Contains(filter) ||
                    x.Age.ToString().Equals(filter) ||
                    x.Roles.Any(x => x.Role.ToString() == filter)).ToList();
            }

            var totalCount = users.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            users = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new List<UserDto>();

            foreach (var user in users)
            {
                response.Add(_mapper.Map<UserDto>(user));
            }

            if (!string.IsNullOrEmpty(orderBy))
            {
                response = orderBy.ToLower() switch
                {
                    "id" => response.OrderBy(x => x.Id).ToList(),
                    "name" => response.OrderBy(x => x.Name).ToList(),
                    "age" => response.OrderBy(x => x.Age).ToList(),
                    "email" => response.OrderBy(x => x.Email).ToList(),
                };
            }

            _logger.LogInformation($"Successful GetAllUsers Request");
            return response;
        }

        [Authorize]
        [HttpGet("GetUserById/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get user by id", Description = "Retrieves a user by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User retrieved by id successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No user found.")]
        public async Task<ActionResult<UserDto>> GetUserByIdAsync(int userId)
        {
            if (userId < 1)
            {
                _logger.LogError("Inavalid user id in GET request");
                return BadRequest("Invalid user id.");
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user is null)
            {
                _logger.LogError("Tried to find not existing user");
                return NotFound($"No user found with id {userId}.");
            }

            return _mapper.Map<UserDto>(user);
        }

        [Authorize]
        [HttpDelete("DeleteUser/{UserId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Delete user by id", Description = "Delete a user by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User deleted by id successfully.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User doesn't exist.")]
        public async Task<IActionResult> DeleteUserAsync(int userId)
        {
            if (userId < 1)
            {
                _logger.LogError("Inavalid user id in DELETE request");
                return BadRequest("Invalid user id.");
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user is null)
            {
                _logger.LogError("Tried to delete not existing user");
                return NotFound($"No user found with id {userId}.");
            }

            await _userService.DeleteUserAsync(user);

            return Ok();
        }

        [Authorize]
        [HttpPut("UpdateUser/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Update user by id", Description = "Update a user by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User updated successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or user with the same email already exists.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User doesn't exist")]
        public async Task<ActionResult<User>> UpdateUserAsync(int userId, [FromBody] UserUpdateDto updateUserDto)
        {
            if (userId < 1)
            {
                _logger.LogError("Inavalid user id in PUT request");
                return BadRequest("Invalid user id.");
            }

            var validationResult = _userValidator.Validate(updateUserDto);

            if (!validationResult.IsValid)
            {
                var stringBuilder = new StringBuilder();
                foreach (var error in validationResult.Errors)
                {
                    stringBuilder.AppendLine(error.ErrorMessage);
                }

                _logger.LogError($"Inavalid PUT request: {Environment.NewLine} {stringBuilder}");
                return BadRequest(stringBuilder.ToString());
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user is null)
            {
                _logger.LogError("Tried to update not existing user");
                return NotFound($"No user found with id {userId}.");
            }

            if (updateUserDto.Email != user.Email)
            {
                var userByEmailInDto = await _userService.GetUserByEmailAsync(updateUserDto.Email);

                if (userByEmailInDto != null)
                {
                    _logger.LogError("Inavlid email in put request");
                    return BadRequest("User with the same email already exists.");
                }
            }

            user.Name = updateUserDto.Name;
            user.Email = updateUserDto.Email;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            user.Age = updateUserDto.Age;

            foreach (var role in updateUserDto.Roles)
            {
                user.Roles.Add(new UserRoles
                {
                    Role = role,
                    UserId = user.Id
                });
            };

            await _userService.UpdateUserAsync(user);

            _logger.LogInformation($"User with id: {userId} updated");
            return Ok(user);
        }

        [Authorize]
        [HttpPut("AddNewRoleToUser/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Add new role to user", Description = "Adds a new role to a user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Role added successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or user already has the role.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User doesn't exist")]
        public async Task<IActionResult> AddRoleToUserAsync(int userId, [Required, FromQuery] Roles newRole)
        {
            if (userId < 1)
            {
                _logger.LogError("Invalid user id in PUT request");
                return BadRequest("Invalid user id.");
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user is null) 
            {
                _logger.LogError("Tried to update not existing user");
                return NotFound($"No user found with id {userId}.");
            }

            var userRoles = user.Roles.Select(x => x.Role);

            if (userRoles.Contains(newRole))
            {
                _logger.LogError($"Tried to add existing role {newRole} to user with id: {userId}");
                return BadRequest($"User has already {newRole} role.");
            }

            user.Roles.Add(new UserRoles { Role = newRole, UserId = user.Id });

            await _userService.UpdateUserAsync(user);

            _logger.LogInformation($"{newRole} role added to user with id: {userId}");
            return Ok(user);
        }
    }
}
