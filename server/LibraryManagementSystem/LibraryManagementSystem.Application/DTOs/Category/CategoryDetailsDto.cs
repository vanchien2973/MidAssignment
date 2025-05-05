namespace LibraryManagementSystem.Application.DTOs.Category;

public class CategoryDetailsDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string Description { get; set; }
    public DateTime CreatedDate { get; set; }
}