using FluentValidation;
using LibraryManagementSystem.Application.Queries.User;

namespace LibraryManagementSystem.Application.Validators.User
{
    public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
    {
        public GetAllUsersQueryValidator()
        {
            RuleFor(query => query.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(query => query.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
        }
    }
} 