using FluentValidation;
using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;

namespace LibraryManagementSystem.Application.Validators.Book
{
    public class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
    {
        private readonly IBookRepository _bookRepository;

        public DeleteBookCommandValidator(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;

            RuleFor(x => x.BookId)
                .NotEmpty().WithMessage("Book ID cannot be empty")
                .MustAsync(BookMustExist)
                .WithMessage("Book does not exist");
        }

        private async Task<bool> BookMustExist(Guid bookId, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            return book != null;
        }
    }
} 