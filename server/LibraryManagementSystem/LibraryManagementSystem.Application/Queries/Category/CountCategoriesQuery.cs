using MediatR;

namespace LibraryManagementSystem.Application.Queries.Category
{
    public class CountCategoriesQuery : IRequest<int>
    {
        public string SearchTerm { get; set; }
        
        public CountCategoriesQuery(string searchTerm = null)
        {
            SearchTerm = searchTerm;
        }
    }
} 