using FluentValidation;
using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Enums;
using System;

namespace LibraryManagementSystem.Application.Validators.Borrowing;

public class ReturnBookCommandValidator : AbstractValidator<ReturnBookCommand>
{
    private readonly IBookBorrowingRequestDetailRepository _borrowingDetailRepository;

    public ReturnBookCommandValidator(IBookBorrowingRequestDetailRepository borrowingDetailRepository)
    {
        _borrowingDetailRepository = borrowingDetailRepository;

        RuleFor(cmd => cmd.DetailId)
            .NotEmpty().WithMessage("Borrowing detail ID cannot be empty.");

        RuleFor(cmd => cmd.UserId)
            .NotEmpty().WithMessage("User ID cannot be empty.");

        RuleFor(cmd => cmd)
            .MustAsync(async (cmd, cancellation) => {
                var detail = await _borrowingDetailRepository.GetByIdAsync(cmd.DetailId);
                return detail != null && 
                      (detail.Status == BorrowingDetailStatus.Borrowing || 
                       detail.Status == BorrowingDetailStatus.Extended);
            }).WithMessage("Can only return books that are in 'Borrowing' or 'Extended' status.");
    }
} 