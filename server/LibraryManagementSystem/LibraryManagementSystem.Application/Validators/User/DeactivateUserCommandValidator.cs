using FluentValidation;
using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Validators.User
{
    public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
    {
        private readonly IUserRepository _userRepository;

        public DeactivateUserCommandValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID cannot be empty")
                .GreaterThan(0).WithMessage("User ID must be greater than 0")
                .MustAsync(UserExistsAndNotSuperUser).WithMessage("User with this ID does not exist or is a SuperUser that cannot be deactivated");
        }

        private async Task<bool> UserExistsAndNotSuperUser(int userId, CancellationToken cancellationToken)
        {
            if (userId <= 0)
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return false;

            return user.UserType != UserType.SuperUser;
        }
    }
} 