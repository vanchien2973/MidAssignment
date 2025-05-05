using FluentValidation;
using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;

namespace LibraryManagementSystem.Application.Validators.Category
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBookRepository _bookRepository;

        public DeleteCategoryCommandValidator(
            ICategoryRepository categoryRepository,
            IBookRepository bookRepository)
        {
            _categoryRepository = categoryRepository;
            _bookRepository = bookRepository;

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID must not be empty")
                .MustAsync(CategoryMustExist)
                .WithMessage("Category does not exist")
                .MustAsync(CategoryMustNotHaveBooks)
                .WithMessage("Don't delete a category that has books associated with it");
        }

        private async Task<bool> CategoryMustExist(Guid categoryId, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            return category != null;
        }

        private async Task<bool> CategoryMustNotHaveBooks(Guid categoryId, CancellationToken cancellationToken)
        {
            return !await _bookRepository.HasBooksInCategoryAsync(categoryId);
        }
    }
} 