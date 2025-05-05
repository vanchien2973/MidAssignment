using MediatR;

namespace LibraryManagementSystem.Application.Commands.Book;

public class DeleteBookCommand : IRequest<bool>
{
    public Guid BookId { get; set; }
} 