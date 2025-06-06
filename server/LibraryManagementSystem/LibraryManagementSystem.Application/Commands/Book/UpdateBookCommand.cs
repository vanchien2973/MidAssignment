using MediatR;

namespace LibraryManagementSystem.Application.Commands.Book;

public class UpdateBookCommand : IRequest<bool>
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
} 