using FluentValidation;
using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Enums;
using System;

namespace LibraryManagementSystem.Application.Validators.Borrowing;

public class UpdateBorrowingRequestStatusCommandValidator : AbstractValidator<UpdateBorrowingRequestStatusCommand>
{
    private readonly IBookBorrowingRequestRepository _borrowingRequestRepository;
    private readonly IBookRepository _bookRepository;

    public UpdateBorrowingRequestStatusCommandValidator(
        IBookBorrowingRequestRepository borrowingRequestRepository,
        IBookRepository bookRepository)
    {
        _borrowingRequestRepository = borrowingRequestRepository;
        _bookRepository = bookRepository;

        RuleFor(cmd => cmd.RequestId)
            .NotEmpty().WithMessage("Request ID cannot be empty.");

        RuleFor(cmd => cmd.ApproverId)
            .NotEmpty().WithMessage("Approver ID cannot be empty.");

        RuleFor(cmd => cmd.Status)
            .NotEmpty().WithMessage("Status cannot be empty.")
            .IsInEnum().WithMessage("Status is invalid.");

        RuleFor(cmd => cmd)
            .MustAsync(async (cmd, cancellation) => {
                var request = await _borrowingRequestRepository.GetByIdAsync(cmd.RequestId);
                return request != null && request.Status == BorrowingRequestStatus.Waiting;
            }).WithMessage("Only requests in waiting status can be updated.");

        When(cmd => cmd.Status == BorrowingRequestStatus.Approved, () => {
            RuleFor(cmd => cmd)
                .MustAsync(async (cmd, cancellation) => {
                    var request = await _borrowingRequestRepository.GetByIdAsync(cmd.RequestId);
                    if (request == null) return false;

                    bool allBooksAvailable = true;
                    foreach (var detail in request.RequestDetails)
                    {
                        var book = await _bookRepository.GetByIdAsync(detail.BookId);
                        if (book == null || book.AvailableCopies <= 0 || !book.IsActive)
                        {
                            allBooksAvailable = false;
                            break;
                        }
                    }

                    return allBooksAvailable;
                }).WithMessage("One or more books in the request are no longer available for borrowing.");
        });
    }
} 