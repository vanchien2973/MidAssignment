using LibraryManagementSystem.Application.DTOs.Book;
using MediatR;

namespace LibraryManagementSystem.Application.Queries.Book
{
    public class GetBooksByCategoryQuery : IRequest<IEnumerable<BookListDto>>
    {
        public Guid CategoryId { get; }
        public int PageNumber { get; }
        public int PageSize { get; }

        public GetBooksByCategoryQuery(Guid categoryId, int pageNumber, int pageSize)
        {
            CategoryId = categoryId;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
} 