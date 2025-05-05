using FluentValidation;
using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Validators.User
{
    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookBorrowingRequestRepository _borrowingRequestRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteUserCommandValidator(
            IUserRepository userRepository, 
            IBookBorrowingRequestRepository borrowingRequestRepository, 
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _borrowingRequestRepository = borrowingRequestRepository;
            _currentUserService = currentUserService;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID cannot be empty")
                .GreaterThan(0).WithMessage("User ID must be greater than 0")
                .Must(NotCurrentUser).WithMessage("You cannot delete your own account")
                .MustAsync(UserMustExist).WithMessage("User with this ID does not exist")
                .MustAsync(UserMustNotHaveActiveLoans).WithMessage("Cannot delete user who has active book loans");
        }

        private bool NotCurrentUser(int userId)
        {
            return userId != _currentUserService.UserId;
        }

        private async Task<bool> UserMustExist(int userId, CancellationToken cancellationToken)
        {
            if (userId <= 0)
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            return user != null;
        }

        private async Task<bool> UserMustNotHaveActiveLoans(int userId, CancellationToken cancellationToken)
        {
            return !await _borrowingRequestRepository.HasUserActiveBookLoansAsync(userId);
        }
    }
} 