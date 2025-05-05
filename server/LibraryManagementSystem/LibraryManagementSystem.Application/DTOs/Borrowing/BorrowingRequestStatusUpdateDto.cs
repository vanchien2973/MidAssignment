using LibraryManagementSystem.Domain.Enums;
using System;

namespace LibraryManagementSystem.Application.DTOs.Borrowing;

public class BorrowingRequestStatusUpdateDto
{
    public Guid RequestId { get; set; }
    public int ApproverId { get; set; }
    public BorrowingRequestStatus Status { get; set; }
    public string Notes { get; set; }
    public int? DueDays { get; set; }
} 