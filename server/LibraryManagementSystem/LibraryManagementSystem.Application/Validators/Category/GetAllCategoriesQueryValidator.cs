using FluentValidation;
using LibraryManagementSystem.Application.Queries.Category;

namespace LibraryManagementSystem.Application.Validators.Category
{
    public class GetAllCategoriesQueryValidator : AbstractValidator<GetAllCategoriesQuery>
    {
        public GetAllCategoriesQueryValidator()
        {
            RuleFor(query => query.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(query => query.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            When(query => !string.IsNullOrEmpty(query.SortBy), () =>
            {
                RuleFor(query => query.SortBy)
                    .Must(sortBy => new[] { "categoryName", "createdDate" }.Contains(sortBy.ToLower()))
                    .WithMessage("Sorting is only allowed by categoryName or createdDate");
            });

            When(query => !string.IsNullOrEmpty(query.SortOrder), () =>
            {
                RuleFor(query => query.SortOrder)
                    .Must(sortOrder => new[] { "asc", "desc" }.Contains(sortOrder.ToLower()))
                    .WithMessage("Sort order can only be asc or desc");
            });
        }
    }
} 