using LibraryManagementSystem.Application.DTOs.Borrowing;
using MediatR;
using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Application.Queries.Borrowing;

public class GetOverdueBorrowingsQuery : IRequest<IEnumerable<BookBorrowingRequestDetailDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
} 