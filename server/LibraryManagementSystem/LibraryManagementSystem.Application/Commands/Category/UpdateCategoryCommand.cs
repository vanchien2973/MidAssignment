using MediatR;

namespace LibraryManagementSystem.Application.Commands.Category;

public class UpdateCategoryCommand : IRequest<bool>
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string Description { get; set; }
} 