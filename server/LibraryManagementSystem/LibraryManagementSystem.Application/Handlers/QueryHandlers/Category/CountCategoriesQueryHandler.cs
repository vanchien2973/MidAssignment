using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Category;
using MediatR;

namespace LibraryManagementSystem.Application.Handlers.Category
{
    public class CountCategoriesQueryHandler : IRequestHandler<CountCategoriesQuery, int>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CountCategoriesQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<int> Handle(CountCategoriesQuery request, CancellationToken cancellationToken)
        {
            return await _categoryRepository.CountAsync(request.SearchTerm);
        }
    }
} 