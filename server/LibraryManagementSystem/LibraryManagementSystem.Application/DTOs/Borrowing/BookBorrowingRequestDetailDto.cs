using LibraryManagementSystem.Domain.Enums;
using System;

namespace LibraryManagementSystem.Application.DTOs.Borrowing;

public class BookBorrowingRequestDetailDto
{
    public Guid DetailId { get; set; }
    public Guid RequestId { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; }
    public string BookAuthor { get; set; }
    public string ISBN { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public BorrowingDetailStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime? ExtensionDate { get; set; }
} 