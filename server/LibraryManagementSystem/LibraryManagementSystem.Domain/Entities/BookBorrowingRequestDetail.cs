using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.Domain.Entities;

public class BookBorrowingRequestDetail
{
    public Guid DetailId { get; set; }
    public Guid RequestId { get; set; }
    public Guid BookId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public BorrowingDetailStatus Status { get; set; }
    public DateTime? ExtensionDate { get; set; }
    
    // Navigation properties
    public BookBorrowingRequest Request { get; set; }
    public Book Book { get; set; }
}