using MediatR;

namespace LibraryManagementSystem.Application.Queries.Book
{
    public class CountBooksByCategoryQuery : IRequest<int>
    {
        public Guid CategoryId { get; }

        public CountBooksByCategoryQuery(Guid categoryId)
        {
            CategoryId = categoryId;
        }
    }
} 