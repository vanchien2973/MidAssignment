using FluentValidation;
using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Enums;
using System;

namespace LibraryManagementSystem.Application.Validators.Borrowing;

public class ExtendBorrowingCommandValidator : AbstractValidator<ExtendBorrowingCommand>
{
    private readonly IBookBorrowingRequestDetailRepository _borrowingDetailRepository;

    public ExtendBorrowingCommandValidator(IBookBorrowingRequestDetailRepository borrowingDetailRepository)
    {
        _borrowingDetailRepository = borrowingDetailRepository;

        RuleFor(cmd => cmd.DetailId)
            .NotEmpty().WithMessage("Borrowing detail ID cannot be empty.");

        RuleFor(cmd => cmd.UserId)
            .NotEmpty().WithMessage("User ID cannot be empty.");

        RuleFor(cmd => cmd.NewDueDate)
            .NotEmpty().WithMessage("New due date cannot be empty.")
            .GreaterThan(DateTime.Now).WithMessage("New due date must be greater than current date.");

        RuleFor(cmd => cmd)
            .MustAsync(async (cmd, cancellation) => {
                var detail = await _borrowingDetailRepository.GetByIdAsync(cmd.DetailId);
                return detail != null && 
                       detail.Status == BorrowingDetailStatus.Borrowing && 
                       detail.ExtensionDate == null;
            }).WithMessage("Can only extend borrowing details that are in 'Borrowing' status and have not been extended before.");
            
        RuleFor(cmd => cmd)
            .MustAsync(async (cmd, cancellation) => {
                var detail = await _borrowingDetailRepository.GetByIdAsync(cmd.DetailId);
                if (detail == null || !detail.DueDate.HasValue) return false;
                
                return cmd.NewDueDate <= detail.DueDate.Value.AddDays(7);
            }).WithMessage("Can only extend for a maximum of 7 days from the original due date.");
    }
} 