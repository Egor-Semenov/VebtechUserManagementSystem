using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public sealed class UserValidator : AbstractValidator<UserRegistrationDto>
    {
        public UserValidator() 
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Age).NotEmpty().WithMessage("Age is required")
                .GreaterThan(0).WithMessage("Age should be positive");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email is incorrect");
            RuleFor(x => x.Roles).ForEach(role => role.IsInEnum()).WithMessage("Invalid user roles");
        }
    }

    public sealed class UserAuthValidator : AbstractValidator<UserAuthDto> 
    {
        public UserAuthValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email is incorrect");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        }
    }
}
