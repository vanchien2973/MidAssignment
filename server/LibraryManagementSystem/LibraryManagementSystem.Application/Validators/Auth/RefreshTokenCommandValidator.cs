using FluentValidation;
using LibraryManagementSystem.Application.Commands.Auth;

namespace LibraryManagementSystem.Application.Validators.Auth
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required");
        }
    }
} 