using LibraryManagementSystem.Domain.Enums;
using MediatR;
using System;

namespace LibraryManagementSystem.Application.Commands.Borrowing;

public class UpdateBorrowingRequestStatusCommand : IRequest<bool>
{
    public Guid RequestId { get; set; }
    public int ApproverId { get; set; }
    public BorrowingRequestStatus Status { get; set; }
    public string Notes { get; set; }
    public int? DueDays { get; set; }
} 