using LibraryManagementSystem.Application.DTOs.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Category;
using MediatR;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.Category
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryListDto>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryListDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortOrder,
                request.SearchTerm);

            return categories.ToCategoryListDTOs();
        }
    }
} 