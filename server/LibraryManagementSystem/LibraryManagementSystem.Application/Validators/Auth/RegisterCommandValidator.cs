using FluentValidation;
using LibraryManagementSystem.Application.Commands.Auth;
using System.Text.RegularExpressions;

namespace LibraryManagementSystem.Application.Validators.Auth
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores and hyphens");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .Must(password => Regex.IsMatch(password, "[A-Z]")).WithMessage("Password must contain at least one uppercase letter")
                .Must(password => Regex.IsMatch(password, "[a-z]")).WithMessage("Password must contain at least one lowercase letter")
                .Must(password => Regex.IsMatch(password, "[0-9]")).WithMessage("Password must contain at least one digit");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format is invalid");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(2).WithMessage("Full name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");
        }
    }
} 