using LibraryManagementSystem.Application.DTOs.Borrowing;
using MediatR;
using System;

namespace LibraryManagementSystem.Application.Queries.Borrowing;

public class GetBorrowingRequestByIdQuery : IRequest<BookBorrowingRequestDto>
{
    public Guid RequestId { get; set; }
} 