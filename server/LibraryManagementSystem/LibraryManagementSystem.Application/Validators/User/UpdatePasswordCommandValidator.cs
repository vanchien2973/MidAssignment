using FluentValidation;
using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using System.Text.RegularExpressions;

namespace LibraryManagementSystem.Application.Validators.User
{
    public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHashService _passwordHashService;

        public UpdatePasswordCommandValidator(
            IUserRepository userRepository,
            IPasswordHashService passwordHashService)
        {
            _userRepository = userRepository;
            _passwordHashService = passwordHashService;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID cannot be empty")
                .GreaterThan(0).WithMessage("User ID must be greater than 0")
                .MustAsync(UserMustExist).WithMessage("User with this ID does not exist");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required")
                .MustAsync(CurrentPasswordMustBeCorrect).WithMessage("Current password is incorrect");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters long")
                .Must(password => Regex.IsMatch(password, "[A-Z]")).WithMessage("Password must contain at least one uppercase letter")
                .Must(password => Regex.IsMatch(password, "[a-z]")).WithMessage("Password must contain at least one lowercase letter")
                .Must(password => Regex.IsMatch(password, "[0-9]")).WithMessage("Password must contain at least one digit")
                .Must((command, newPassword) => newPassword != command.CurrentPassword)
                .WithMessage("New password must be different from the current password");
        }

        private async Task<bool> UserMustExist(int userId, CancellationToken cancellationToken)
        {
            if (userId <= 0)
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            return user != null;
        }

        private async Task<bool> CurrentPasswordMustBeCorrect(UpdatePasswordCommand command, string currentPassword, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(currentPassword))
                return false;

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
                return false;

            return _passwordHashService.VerifyPassword(currentPassword, user.Password);
        }
    }
}