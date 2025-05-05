using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.DTOs.Category;
using LibraryManagementSystem.Application.Queries.Category;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Mappers
{
    public static class CategoryMapper
    {
        // Command Mappers
        public static CreateCategoryCommand ToCommand(this CategoryCreateDto dto)
        {
            return new CreateCategoryCommand
            {
                CategoryName = dto.CategoryName,
                Description = dto.Description
            };
        }

        public static UpdateCategoryCommand ToCommand(this CategoryUpdateDto dto)
        {
            return new UpdateCategoryCommand
            {
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                Description = dto.Description
            };
        }

        public static DeleteCategoryCommand ToCommand(this Guid categoryId)
        {
            return new DeleteCategoryCommand
            {
                CategoryId = categoryId
            };
        }

        // Query Mappers
        public static GetCategoryByIdQuery ToQuery(this Guid categoryId)
        {
            return new GetCategoryByIdQuery(categoryId);
        }

        public static GetAllCategoriesQuery ToQuery(this CategoryQueryParametersDto parameters)
        {
            return new GetAllCategoriesQuery(
                parameters.PageNumber,
                parameters.PageSize,
                parameters.SortBy,
                parameters.SortOrder,
                parameters.SearchTerm);
        }

        public static CountCategoriesQuery ToCountQuery(string searchTerm = null)
        {
            return new CountCategoriesQuery(searchTerm);
        }

        // Entity Mappers
        public static Category ToEntity(this CreateCategoryCommand command)
        {
            return new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = command.CategoryName,
                Description = command.Description,
                CreatedDate = DateTime.UtcNow
            };
        }

        public static void UpdateFrom(this Category category, UpdateCategoryCommand command)
        {
            category.CategoryName = command.CategoryName;
            category.Description = command.Description;
        }

        // Entity To DTO Mappings
        public static CategoryListDto ToCategoryListDTO(this Category category)
        {
            return new CategoryListDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description
            };
        }

        public static CategoryDetailsDto ToCategoryDetailsDTO(this Category category)
        {
            return new CategoryDetailsDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                CreatedDate = category.CreatedDate
            };
        }

        public static IEnumerable<CategoryListDto> ToCategoryListDTOs(this IEnumerable<Category> categories)
        {
            return categories.Select(ToCategoryListDTO);
        }
    }
} 