using FluentValidation;
using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using System.Text.RegularExpressions;

namespace LibraryManagementSystem.Application.Validators.Category
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;

        public UpdateCategoryCommandValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID cannot be empty")
                .MustAsync(CategoryMustExist)
                .WithMessage("Category does not exist");

            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Category name cannot be empty")
                .MinimumLength(3).WithMessage("Category name must have at least 3 characters")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters")
                .Must(name => !Regex.IsMatch(name, @"^\s+$"))
                .WithMessage("Category name cannot contain only whitespace")
                .MustAsync(CategoryNameMustBeUnique)
                .WithMessage("Category name already exists in the system");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }

        private async Task<bool> CategoryMustExist(Guid categoryId, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            return category != null;
        }

        private async Task<bool> CategoryNameMustBeUnique(UpdateCategoryCommand command, string categoryName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return true;

            var existingCategory = await _categoryRepository.GetByIdAsync(command.CategoryId);
            if (existingCategory != null && existingCategory.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                return true;

            return !await _categoryRepository.NameExistsAsync(categoryName);
        }
    }
} 