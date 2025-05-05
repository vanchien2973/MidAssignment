using MediatR;
using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Application.Commands.Borrowing;

public class CreateBorrowingRequestCommand : IRequest<Guid>
{
    public int RequestorId { get; set; }
    public string Notes { get; set; }
    public List<BorrowingBookItem> Books { get; set; } = new List<BorrowingBookItem>();
}

public class BorrowingBookItem
{
    public Guid BookId { get; set; }
} 