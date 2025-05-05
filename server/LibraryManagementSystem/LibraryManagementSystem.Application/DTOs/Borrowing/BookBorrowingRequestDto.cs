using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Domain.Enums;
using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Application.DTOs.Borrowing;

public class BookBorrowingRequestDto
{
    public Guid RequestId { get; set; }
    public int RequestorId { get; set; }
    public string RequestorName { get; set; }
    public DateTime RequestDate { get; set; }
    public BorrowingRequestStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int? ApproverId { get; set; }
    public string ApproverName { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string Notes { get; set; }
    public IList<BookBorrowingRequestDetailDto> RequestDetails { get; set; }
} 