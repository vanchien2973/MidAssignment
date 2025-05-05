using MediatR;
using LibraryManagementSystem.Application.DTOs.Book;
using System.Collections.Generic;

namespace LibraryManagementSystem.Application.Queries.Book
{
    public class GetAllBooksQuery : IRequest<IEnumerable<BookListDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public string SortOrder { get; set; }

        public GetAllBooksQuery(int pageNumber = 1, int pageSize = 10, string sortBy = null, string sortOrder = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SortBy = sortBy;
            SortOrder = sortOrder;
        }
    }
} 