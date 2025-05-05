using FluentValidation;
using LibraryManagementSystem.Application.Queries.Auth;

namespace LibraryManagementSystem.Application.Validators.Auth
{
    public class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
    {
        public GetCurrentUserQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .Must(userId => int.TryParse(userId, out _)).WithMessage("User ID must be a valid integer");
        }
    }
} 