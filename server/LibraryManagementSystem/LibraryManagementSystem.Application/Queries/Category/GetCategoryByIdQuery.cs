using MediatR;
using LibraryManagementSystem.Application.DTOs.Category;
using System;

namespace LibraryManagementSystem.Application.Queries.Category
{
    public class GetCategoryByIdQuery : IRequest<CategoryDetailsDto>
    {
        public Guid CategoryId { get; }

        public GetCategoryByIdQuery(Guid categoryId)
        {
            CategoryId = categoryId;
        }
    }
} 