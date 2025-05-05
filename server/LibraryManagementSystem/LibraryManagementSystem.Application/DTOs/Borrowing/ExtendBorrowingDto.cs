using System;

namespace LibraryManagementSystem.Application.DTOs.Borrowing;

public class ExtendBorrowingDto
{
    public Guid DetailId { get; set; }
    public int UserId { get; set; } // Người thực hiện việc gia hạn
    public DateTime NewDueDate { get; set; }
    public string? Notes { get; set; }
} 