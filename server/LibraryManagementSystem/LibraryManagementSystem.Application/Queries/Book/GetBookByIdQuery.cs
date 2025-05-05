using MediatR;
using LibraryManagementSystem.Application.DTOs.Book;

namespace LibraryManagementSystem.Application.Queries.Book
{
    public class GetBookByIdQuery : IRequest<BookDetailsDto>
    {
        public Guid BookId { get; private set; }

        public GetBookByIdQuery(Guid bookId)
        {
            BookId = bookId;
        }
    }
} 