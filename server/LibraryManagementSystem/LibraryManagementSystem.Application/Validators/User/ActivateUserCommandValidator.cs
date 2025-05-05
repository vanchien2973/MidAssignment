using FluentValidation;
using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Validators.User
{
    public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
    {
        private readonly IUserRepository _userRepository;

        public ActivateUserCommandValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID cannot be empty")
                .GreaterThan(0).WithMessage("User ID must be greater than 0")
                .MustAsync(UserMustExist).WithMessage("User with this ID does not exist");
        }

        private async Task<bool> UserMustExist(int userId, CancellationToken cancellationToken)
        {
            if (userId <= 0)
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            return user != null;
        }
    }
} 