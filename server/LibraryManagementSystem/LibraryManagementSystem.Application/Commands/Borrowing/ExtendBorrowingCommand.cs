using MediatR;
using System;

namespace LibraryManagementSystem.Application.Commands.Borrowing;

public class ExtendBorrowingCommand : IRequest<bool>
{
    public Guid DetailId { get; set; }
    public int UserId { get; set; }
    public DateTime NewDueDate { get; set; }
    public string? Notes { get; set; }
} 