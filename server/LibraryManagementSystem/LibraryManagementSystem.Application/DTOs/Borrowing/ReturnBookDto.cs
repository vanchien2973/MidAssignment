using System;

namespace LibraryManagementSystem.Application.DTOs.Borrowing;

public class ReturnBookDto
{
    public Guid DetailId { get; set; }
    public int UserId { get; set; } // Người thực hiện việc trả sách (staff hoặc người dùng)
    public string? Notes { get; set; }
} 