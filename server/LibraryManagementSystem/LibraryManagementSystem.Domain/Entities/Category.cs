namespace LibraryManagementSystem.Domain.Entities;

public class Category
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string Description { get; set; }
    public DateTime CreatedDate { get; set; }
    
    // Navigation properties
    public ICollection<Book> Books { get; set; }
}