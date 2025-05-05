using MediatR;

namespace LibraryManagementSystem.Application.Commands.Category;

public class DeleteCategoryCommand : IRequest<bool>
{
    public Guid CategoryId { get; set; }
} 