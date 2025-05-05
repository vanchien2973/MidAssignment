namespace LibraryManagementSystem.Application.DTOs.Category;

public class CategoryUpdateDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string Description { get; set; }
}