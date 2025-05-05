using FluentValidation;
using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Validators.User
{
    public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateUserRoleCommandValidator(IUserRepository userRepository, ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID cannot be empty")
                .GreaterThan(0).WithMessage("User ID must be greater than 0")
                .MustAsync(UserMustExist).WithMessage("User with this ID does not exist");

            RuleFor(x => x)
                .Must(x => x.UserId != _currentUserService.UserId)
                .WithMessage("You cannot change your own role");
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