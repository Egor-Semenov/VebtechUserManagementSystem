using Application.DTOs;
using Application.Filters;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace VebtechTask.Api.Controllers
{
    [Route("api/user-management")]
    [ApiController]
    public sealed class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService, 
            IMapper mapper,
            ILogger<UserController> logger
            )
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("register-user")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Register new user", Description = "Register a new user.")]
        [SwaggerResponse(StatusCodes.Status201Created, "New user registered successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or user with the same email already exists")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto userRegistrationDto)
        {          
            await _userService.CreateUserAsync(userRegistrationDto);

            _logger.LogInformation($"User created");
            return StatusCode(201);
        }

        [Authorize]
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = "Get all users", Description = "Retrieves a list of all users.")]
        [SwaggerResponse(StatusCodes.Status200OK, "List of users retrieved successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User isn't authorized.")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers(
            [FromQuery] UserQueryFilterModel filterModel,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
            )     
        {
            var users = await _userService.GetUsersAsync(filterModel, page, pageSize);

            _logger.LogInformation($"Successful GetAllUsers Request");
            return Ok(users);
        }

        [Authorize]
        [HttpGet("users/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get user by id", Description = "Retrieves a user by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User retrieved by id successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User isn't authorized.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No user found.")]
        public async Task<ActionResult<UserDto>> GetUserByIdAsync(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            _logger.LogInformation($"Get data about user with id: {userId} successful");
            return _mapper.Map<UserDto>(user);
        }

        [Authorize]
        [HttpDelete("delete-user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Delete user by id", Description = "Delete a user by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User deleted by id successfully.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User isn't authorized.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User doesn't exist.")]
        public async Task<IActionResult> DeleteUserAsync(int userId)
        {
            await _userService.DeleteUserAsync(userId);
            _logger.LogInformation($"User with id: {userId} is deleted");
            return Ok();
        }

        [Authorize]
        [HttpPut("update-user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Update user by id", Description = "Update a user by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User updated successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or user with the same email already exists.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User isn't authorized.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User doesn't exist")]
        public async Task<IActionResult> UpdateUserAsync(int userId, [FromBody] UserUpdateDto updateUserDto)
        {
            await _userService.UpdateUserAsync(userId, updateUserDto);

            _logger.LogInformation($"User with id: {userId} updated");
            return Ok();
        }

        [Authorize]
        [HttpPut("add-role/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Add new role to user", Description = "Adds a new role to a user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Role added successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or user already has the role.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User isn't authorized.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User doesn't exist")]
        public async Task<IActionResult> AddRoleToUserAsync(int userId, [Required, FromQuery] Roles newRole)
        {
            await _userService.AddNewRoleToUserAsync(userId, newRole);

            _logger.LogInformation($"{newRole} role added to user with id: {userId}");
            return Ok();
        }
    }
}
