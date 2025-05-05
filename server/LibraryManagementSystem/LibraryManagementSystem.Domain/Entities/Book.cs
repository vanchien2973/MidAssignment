namespace LibraryManagementSystem.Domain.Entities;

public class Book
{
    public Guid BookId { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public Guid CategoryId { get; set; }
    public string ISBN { get; set; }
    public int? PublishedYear { get; set; }
    public string Publisher { get; set; }
    public string Description { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public Category Category { get; set; }
    public ICollection<BookBorrowingRequestDetail> BorrowingDetails { get; set; }
}