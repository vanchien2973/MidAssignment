using FluentValidation;
using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using System;
using System.Linq;

namespace LibraryManagementSystem.Application.Validators.Borrowing;

public class CreateBorrowingRequestCommandValidator : AbstractValidator<CreateBorrowingRequestCommand>
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookBorrowingRequestRepository _borrowingRequestRepository;

    public CreateBorrowingRequestCommandValidator(
        IBookRepository bookRepository,
        IBookBorrowingRequestRepository borrowingRequestRepository)
    {
        _bookRepository = bookRepository;
        _borrowingRequestRepository = borrowingRequestRepository;

        RuleFor(cmd => cmd.RequestorId)
            .NotEmpty().WithMessage("Requestor ID cannot be empty.");

        RuleFor(cmd => cmd.Books)
            .NotEmpty().WithMessage("You must select at least one book to borrow.")
            .Must(books => books.Count <= 5).WithMessage("Cannot borrow more than 5 books in one request.");

        RuleFor(cmd => cmd)
            .MustAsync(async (cmd, cancellation) => {
                var now = DateTime.UtcNow;
                var monthStart = new DateTime(now.Year, now.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                var requestCount = await _borrowingRequestRepository
                    .GetRequestCountByUserInMonthAsync(cmd.RequestorId, monthStart, monthEnd);
                
                return requestCount < 3;
            }).WithMessage("Users cannot create more than 3 borrowing requests in a month.");

        RuleForEach(cmd => cmd.Books)
            .MustAsync(async (book, cancellation) => {
                var bookEntity = await _bookRepository.GetByIdAsync(book.BookId);
                return bookEntity != null && bookEntity.AvailableCopies > 0 && bookEntity.IsActive;
            }).WithMessage("One or more books are not available for borrowing.");

        RuleFor(cmd => cmd)
            .MustAsync(async (cmd, cancellation) => {
                var hasActiveLoans = await _borrowingRequestRepository.HasUserActiveBookLoansAsync(cmd.RequestorId);
                return !hasActiveLoans;
            }).WithMessage("User has overdue or unreturned books. Please return all books before making new requests.");
    }
} 