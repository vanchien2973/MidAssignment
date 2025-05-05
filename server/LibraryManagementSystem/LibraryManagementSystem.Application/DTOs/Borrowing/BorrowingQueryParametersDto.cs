using LibraryManagementSystem.Domain.Enums;
using System;

namespace LibraryManagementSystem.Application.DTOs.Borrowing;

public class BorrowingQueryParametersDto
{
    public int? UserId { get; set; }
    public BorrowingRequestStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "RequestDate";
    public string SortOrder { get; set; } = "desc";
} 