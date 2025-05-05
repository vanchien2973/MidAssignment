using FluentValidation;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;

namespace LibraryManagementSystem.Application.Validators.Book
{
    public class GetBookByIdQueryValidator : AbstractValidator<GetBookByIdQuery>
    {
        private readonly IBookRepository _bookRepository;

        public GetBookByIdQueryValidator(IBookRepository bookRepository)
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