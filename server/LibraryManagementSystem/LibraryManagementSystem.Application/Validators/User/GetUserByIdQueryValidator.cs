using FluentValidation;
using LibraryManagementSystem.Application.Queries.User;

namespace LibraryManagementSystem.Application.Validators.User
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(query => query.UserId)
                .NotEmpty().WithMessage("User ID cannot be empty")
                .GreaterThan(0).WithMessage("User ID must be a positive integer");
        }
    }
} 