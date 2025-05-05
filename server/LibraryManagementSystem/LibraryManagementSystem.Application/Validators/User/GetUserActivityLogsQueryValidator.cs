using FluentValidation;
using LibraryManagementSystem.Application.Queries.User;

namespace LibraryManagementSystem.Application.Validators.User
{
    public class GetUserActivityLogsQueryValidator : AbstractValidator<GetUserActivityLogsQuery>
    {
        public GetUserActivityLogsQueryValidator()
        {
            RuleFor(query => query.UserId)
                .GreaterThan(0).WithMessage("User ID must be a positive integer");

            RuleFor(query => query.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(query => query.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            When(query => !string.IsNullOrEmpty(query.ActivityType), () =>
            {
                RuleFor(query => query.ActivityType)
                    .Must(activityType => new[] { "login", "logout", "borrow", "return", "admin" }
                        .Contains(activityType.ToLower()))
                    .WithMessage("Invalid activity type. Valid types: login, logout, borrow, return, admin");
            });
        }
    }
} 