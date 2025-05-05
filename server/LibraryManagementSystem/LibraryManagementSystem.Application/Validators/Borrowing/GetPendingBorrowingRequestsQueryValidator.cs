using FluentValidation;
using LibraryManagementSystem.Application.Queries.Borrowing;
using System;

namespace LibraryManagementSystem.Application.Validators.Borrowing;

public class GetPendingBorrowingRequestsQueryValidator : AbstractValidator<GetPendingBorrowingRequestsQuery>
{
    public GetPendingBorrowingRequestsQueryValidator()
    {
        RuleFor(q => q.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(q => q.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Page size cannot exceed 50.");
    }
} 