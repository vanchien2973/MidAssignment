using MediatR;
using LibraryManagementSystem.Application.DTOs.Category;
using System.Collections.Generic;

namespace LibraryManagementSystem.Application.Queries.Category
{
    public class GetAllCategoriesQuery : IRequest<IEnumerable<CategoryListDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string SearchTerm { get; set; }

        public GetAllCategoriesQuery(int pageNumber = 1, int pageSize = 10, string sortBy = null, string sortOrder = null, string searchTerm = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SortBy = sortBy;
            SortOrder = sortOrder;
            SearchTerm = searchTerm;
        }
    }
} 