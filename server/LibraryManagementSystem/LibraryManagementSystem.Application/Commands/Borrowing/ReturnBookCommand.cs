using MediatR;
using System;

namespace LibraryManagementSystem.Application.Commands.Borrowing;

public class ReturnBookCommand : IRequest<bool>
{
    public Guid DetailId { get; set; }
    public int UserId { get; set; }
    public string? Notes { get; set; }
} 