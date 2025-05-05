using FluentValidation;
using LibraryManagementSystem.Application.Queries.Book;

namespace LibraryManagementSystem.Application.Validators.Book
{
    public class GetAllBooksQueryValidator : AbstractValidator<GetAllBooksQuery>
    {
        public GetAllBooksQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(50).WithMessage("Page size cannot exceed 50");

            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) || 
                                IsValidSortField(sortBy))
                .WithMessage("Invalid sort field. Valid fields are: Title, Author, PublishedYear, Publisher, CategoryName");

            RuleFor(x => x.SortOrder)
                .Must(sortOrder => string.IsNullOrEmpty(sortOrder) || 
                                  sortOrder.ToLower() == "asc" || 
                                  sortOrder.ToLower() == "desc")
                .WithMessage("Sort order must be either 'asc' or 'desc'");
        }

        private bool IsValidSortField(string sortBy)
        {
            var validFields = new[] { "title", "author", "publishedyear", "publisher", "categoryname" };
            return validFields.Contains(sortBy.ToLower());
        }
    }
} 