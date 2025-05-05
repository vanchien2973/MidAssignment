using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.Domain.Entities;

public class BookBorrowingRequest
{
    public Guid RequestId { get; set; }
    public int RequestorId { get; set; }
    public DateTime RequestDate { get; set; }
    public BorrowingRequestStatus Status { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string Notes { get; set; }
    
    // Navigation properties
    public User Requestor { get; set; }
    public User Approver { get; set; }
    public ICollection<BookBorrowingRequestDetail> RequestDetails { get; set; }
}