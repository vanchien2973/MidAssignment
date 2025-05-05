using FluentValidation;
using LibraryManagementSystem.Application.Queries.Borrowing;
using System;

namespace LibraryManagementSystem.Application.Validators.Borrowing;

public class GetBorrowingRequestByIdQueryValidator : AbstractValidator<GetBorrowingRequestByIdQuery>
{
    public GetBorrowingRequestByIdQueryValidator()
    {
        RuleFor(q => q.RequestId)
            .NotEmpty().WithMessage("Borrowing request ID cannot be empty.");
    }
} 