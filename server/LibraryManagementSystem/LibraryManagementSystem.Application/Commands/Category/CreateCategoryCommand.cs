using MediatR;

namespace LibraryManagementSystem.Application.Commands.Category;

public class CreateCategoryCommand : IRequest<bool>
{
    public string CategoryName { get; set; }
    public string Description { get; set; }
} 