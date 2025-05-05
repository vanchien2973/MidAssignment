using FluentValidation;
using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Validators.User
{
    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserProfileCommandValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID cannot be empty")
                .GreaterThan(0).WithMessage("User ID must be greater than 0")
                .MustAsync(UserMustExist).WithMessage("User with this ID does not exist");

            When(x => !string.IsNullOrEmpty(x.Email), () =>
            {
                RuleFor(x => x.Email)
                    .Must(BeValidEmail).WithMessage("Invalid email format")
                    .MustAsync(EmailMustBeUnique).WithMessage("Email already exists");
            });

            When(x => !string.IsNullOrEmpty(x.FullName), () =>
            {
                RuleFor(x => x.FullName)
                    .Must(fullName => !Regex.IsMatch(fullName, @"^\s+$"))
                    .WithMessage("Full name cannot contain only whitespace");
            });
        }

        private async Task<bool> UserMustExist(int userId, CancellationToken cancellationToken)
        {
            if (userId <= 0)
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            return user != null;
        }

        private bool BeValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> EmailMustBeUnique(UpdateUserProfileCommand command, string email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            return !await _userRepository.EmailExistsAsync(email, command.UserId);
        }
    }
} 