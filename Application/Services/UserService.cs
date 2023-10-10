using Application.DTOs;
using Application.Filters;
using Application.Middleware;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Models.Entities;
using Domain.Models.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Services
{
    public sealed class UserService : IUserService
    {
        private readonly IBaseRepository<User> _userRepository;
        private readonly IValidator<UserRegistrationDto> _userRegistrationValidator;
        private readonly IValidator<UserUpdateDto> _userUpdateValidator;
        private readonly IMapper _mapper;

        public UserService(
            IBaseRepository<User> userRepository, 
            IValidator<UserRegistrationDto> userRegistrationValidator,
            IValidator<UserUpdateDto> userUpdateValidator,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _userRegistrationValidator = userRegistrationValidator;
            _userUpdateValidator = userUpdateValidator;
            _mapper = mapper;
        }

        public async Task CreateUserAsync(UserRegistrationDto userRegistrationDto)
        {
            var validationResult = _userRegistrationValidator.Validate(userRegistrationDto);
            if (!validationResult.IsValid)
            {
                var stringBuilder = new StringBuilder();
                foreach (var error in validationResult.Errors)
                {
                    stringBuilder.AppendLine(error.ErrorMessage);
                }

                throw new BadRequestException(stringBuilder.ToString());
            }

            await ValidateUserUniqueEmail(userRegistrationDto.Email);

            var user = _mapper.Map<User>(userRegistrationDto);

            foreach (var role in userRegistrationDto.Roles)
            {
                user.Roles.Add(new UserRoles
                {
                    Role = role,
                    UserId = user.Id
                });
            };

            await _userRepository.Create(user);
        }

        public async Task DeleteUserAsync(int userId)
        {
            if (userId < 1)
            {
                throw new BadRequestException("Invalid user id.");
            }

            var user = await GetUserByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException($"No user found with id {userId}.");
            }

            await _userRepository.Delete(user);
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            if (id < 1)
            {
                throw new BadRequestException("Invalid user id.");
            }

            var user = await _userRepository.FindByCondition(x => x.Id == id).FirstOrDefaultAsync();

            if (user is null)
            {
                throw new NotFoundException($"No user found with id {id}.");
            }

            return user;
        }

        public Task<User> GetUserByEmailAsync(string email) =>
            _userRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();

        public async Task<List<UserDto>> GetUsersAsync(UserQueryFilterModel filterModel, int page, int pageSize)
        {
            var userQuery = await _userRepository.FindAll().ToListAsync();

            if (!string.IsNullOrEmpty(filterModel.Name))
            {
                userQuery = userQuery.Where(x => x.Name.Contains(filterModel.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (filterModel.Age is not null)
            {
                userQuery = userQuery.Where(x => x.Age == filterModel.Age).ToList();
            }

            if (!string.IsNullOrEmpty(filterModel.Email))
            {
                userQuery = userQuery.Where(x => x.Email.Contains(filterModel.Email, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (filterModel.Role != null || filterModel.Role == 0)
            {
                userQuery = userQuery.Where(x => x.Roles.Select(x => x.Role).ToList().Contains(filterModel.Role.Value)).ToList();
            }

            var totalCount = userQuery.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            userQuery = userQuery.Skip((page - 1) * pageSize)
                .Take(pageSize).ToList();

            return _mapper.Map<List<UserDto>>(userQuery);
        }

        public Task UpdateUserAsync(User user) =>
            _userRepository.Update(user);

        public async Task UpdateUserAsync(int userId, UserUpdateDto userUpdateDto)
        {
            if (userId < 1)
            {
                throw new BadRequestException("Invalid user id.");
            }

            var validationResult = _userUpdateValidator.Validate(userUpdateDto);

            if (!validationResult.IsValid)
            {
                var stringBuilder = new StringBuilder();
                foreach (var error in validationResult.Errors)
                {
                    stringBuilder.AppendLine(error.ErrorMessage);
                }

                throw new BadRequestException(stringBuilder.ToString());
            }

            var user = await GetUserByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException($"No user found with id {userId}.");
            }

            if (userUpdateDto.Email.ToLower() != user.Email.ToLower())
            {
                await ValidateUserUniqueEmail(userUpdateDto.Email);
            }

            user.Name = userUpdateDto.Name;
            user.Email = userUpdateDto.Email.ToLower();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userUpdateDto.Password);
            user.Age = userUpdateDto.Age;
            user.Roles = new List<UserRoles>();

            foreach (var role in userUpdateDto.Roles)
            {
                user.Roles.Add(new UserRoles
                {
                    Role = role,
                    UserId = user.Id
                });
            };

            await _userRepository.Update(user);
        }

        public async Task AddNewRoleToUserAsync(int userId, Roles newRole)
        {
            if (userId < 1)
            {
                throw new BadRequestException("Invalid user id.");
            }

            var user = await GetUserByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException($"No user found with id {userId}.");
            }

            var userRoles = user.Roles.Select(x => x.Role).ToList();

            if (userRoles.Contains(newRole))
            {
                throw new BadRequestException($"User has already {newRole} role.");
            }

            user.Roles.Add(new UserRoles { Role = newRole, UserId = user.Id });

            await _userRepository.Update(user);
        }

        private async Task ValidateUserUniqueEmail(string email)
        {
            var userByEmailInDto = await GetUserByEmailAsync(email);

            if (userByEmailInDto != null)
            {
                throw new BadRequestException("User with the same email already exists.");
            }
        }
    }
}
