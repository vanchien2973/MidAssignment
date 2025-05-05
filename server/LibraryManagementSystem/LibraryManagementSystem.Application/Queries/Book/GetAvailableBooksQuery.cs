using LibraryManagementSystem.Application.DTOs.Book;
using MediatR;

namespace LibraryManagementSystem.Application.Queries.Book
{
    public class GetAvailableBooksQuery : IRequest<IEnumerable<BookListDto>>
    {
        public int PageNumber { get; }
        public int PageSize { get; }

        public GetAvailableBooksQuery(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
} 