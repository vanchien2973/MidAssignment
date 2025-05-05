using FluentValidation;
using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using System.Text.RegularExpressions;

namespace LibraryManagementSystem.Application.Validators.Book
{
    public class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;

        public UpdateBookCommandValidator(IBookRepository bookRepository, ICategoryRepository categoryRepository)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;

            RuleFor(x => x.BookId)
                .NotEmpty().WithMessage("Book ID cannot be empty")
                .MustAsync(BookMustExist)
                .WithMessage("Book does not exist");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty")
                .MinimumLength(2).WithMessage("Title must have at least 2 characters")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author cannot be empty")
                .MinimumLength(2).WithMessage("Author name must have at least 2 characters")
                .MaximumLength(100).WithMessage("Author name cannot exceed 100 characters");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID cannot be empty")
                .MustAsync(CategoryMustExist)
                .WithMessage("Selected category does not exist");

            RuleFor(x => x.ISBN)
                .NotEmpty().WithMessage("ISBN cannot be empty")
                .Matches(@"^\d{13}$").WithMessage("ISBN must be a valid 13-digit format")
                .MustAsync(IsbnMustBeUnique)
                .WithMessage("ISBN already exists in the system");

            RuleFor(x => x.PublishedYear)
                .InclusiveBetween(1000, DateTime.Now.Year).When(x => x.PublishedYear.HasValue)
                .WithMessage($"Published year must be between 1000 and {DateTime.Now.Year}");

            RuleFor(x => x.Publisher)
                .MaximumLength(100).WithMessage("Publisher name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.TotalCopies)
                .GreaterThan(0).WithMessage("Total copies must be greater than 0");
        }

        private async Task<bool> BookMustExist(Guid bookId, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            return book != null;
        }

        private async Task<bool> CategoryMustExist(Guid categoryId, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            return category != null;
        }

        private async Task<bool> IsbnMustBeUnique(UpdateBookCommand command, string isbn, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return true;

            var existingBook = await _bookRepository.GetByIdAsync(command.BookId);
            if (existingBook != null && existingBook.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase))
                return true;

            return !await _bookRepository.IsbnExistsAsync(isbn);
        }
    }
} 