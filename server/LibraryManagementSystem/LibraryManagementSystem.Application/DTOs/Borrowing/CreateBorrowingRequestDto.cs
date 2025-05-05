using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Application.DTOs.Borrowing;

public class CreateBorrowingRequestDto
{
    public int RequestorId { get; set; }
    public string Notes { get; set; }
    public List<BorrowingBookItemDto> Books { get; set; } = new List<BorrowingBookItemDto>();
}

public class BorrowingBookItemDto
{
    public Guid BookId { get; set; }
} 