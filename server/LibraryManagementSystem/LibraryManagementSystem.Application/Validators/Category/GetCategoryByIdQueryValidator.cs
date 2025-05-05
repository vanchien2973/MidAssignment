using FluentValidation;
using LibraryManagementSystem.Application.Queries.Category;

namespace LibraryManagementSystem.Application.Validators.Category
{
    public class GetCategoryByIdQueryValidator : AbstractValidator<GetCategoryByIdQuery>
    {
        public GetCategoryByIdQueryValidator()
        {
            RuleFor(query => query.CategoryId)
                .NotEmpty().WithMessage("Category ID cannot be empty")
                .NotEqual(Guid.Empty).WithMessage("Category ID is not valid");
        }
    }
} 