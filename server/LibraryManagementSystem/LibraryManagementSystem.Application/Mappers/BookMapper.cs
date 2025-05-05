using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Queries.Book;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Mappers
{
    public static class BookMapper
    {
        // Command Mappers
        public static CreateBookCommand ToCommand(this BookCreateDto dto)
        {
            return new CreateBookCommand
            {
                Title = dto.Title,
                Author = dto.Author,
                CategoryId = dto.CategoryId,
                ISBN = dto.ISBN,
                PublishedYear = dto.PublishedYear,
                Publisher = dto.Publisher,
                Description = dto.Description,
                TotalCopies = dto.TotalCopies
            };
        }

        public static UpdateBookCommand ToCommand(this BookUpdateDto dto)
        {
            return new UpdateBookCommand
            {
                BookId = dto.BookId,
                Title = dto.Title,
                Author = dto.Author,
                CategoryId = dto.CategoryId,
                ISBN = dto.ISBN,
                PublishedYear = dto.PublishedYear,
                Publisher = dto.Publisher,
                Description = dto.Description,
                TotalCopies = dto.TotalCopies
            };
        }

        public static DeleteBookCommand ToCommand(this Guid bookId)
        {
            return new DeleteBookCommand
            {
                BookId = bookId
            };
        }

        // Query Mappers
        public static GetBookByIdQuery ToQuery(this Guid bookId)
        {
            return new GetBookByIdQuery(bookId);
        }

        public static GetAllBooksQuery ToQuery(this BookQueryParametersDto parameters)
        {
            return new GetAllBooksQuery(
                parameters.PageNumber,
                parameters.PageSize,
                parameters.SortBy,
                parameters.SortOrder);
        }

        public static CountBooksQuery ToCountQuery()
        {
            return new CountBooksQuery();
        }

        public static GetBooksByCategoryQuery ToBooksByCategoryQuery(Guid categoryId, int pageNumber, int pageSize)
        {
            return new GetBooksByCategoryQuery(categoryId, pageNumber, pageSize);
        }

        public static CountBooksByCategoryQuery ToBooksByCategoryCountQuery(Guid categoryId)
        {
            return new CountBooksByCategoryQuery(categoryId);
        }

        public static GetAvailableBooksQuery ToAvailableBooksQuery(int pageNumber, int pageSize)
        {
            return new GetAvailableBooksQuery(pageNumber, pageSize);
        }

        public static CountAvailableBooksQuery ToAvailableBooksCountQuery()
        {
            return new CountAvailableBooksQuery();
        }

        // Entity Mappers
        public static Book ToEntity(this CreateBookCommand command)
        {
            return new Book
            {
                BookId = Guid.NewGuid(),
                Title = command.Title,
                Author = command.Author,
                CategoryId = command.CategoryId,
                ISBN = command.ISBN,
                PublishedYear = command.PublishedYear,
                Publisher = command.Publisher,
                Description = command.Description,
                TotalCopies = command.TotalCopies,
                AvailableCopies = command.TotalCopies, // Initially all copies are available
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
        }

        public static void UpdateFrom(this Book book, UpdateBookCommand command)
        {
            book.Title = command.Title;
            book.Author = command.Author;
            book.CategoryId = command.CategoryId;
            book.ISBN = command.ISBN;
            book.PublishedYear = command.PublishedYear;
            book.Publisher = command.Publisher;
            book.Description = command.Description;
            
            // Adjust available copies based on the change in total copies
            int difference = command.TotalCopies - book.TotalCopies;
            book.TotalCopies = command.TotalCopies;
            book.AvailableCopies += difference;
            
            // Ensure available copies is not negative
            if (book.AvailableCopies < 0)
                book.AvailableCopies = 0;
        }

        // Entity To DTO Mappings
        public static BookListDto ToBookListDTO(this Book book)
        {
            return new BookListDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishedYear = book.PublishedYear,
                Publisher = book.Publisher,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                CategoryId = book.CategoryId,
                CategoryName = book.Category?.CategoryName
            };
        }

        public static BookDetailsDto ToBookDetailsDTO(this Book book)
        {
            return new BookDetailsDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishedYear = book.PublishedYear,
                Publisher = book.Publisher,
                Description = book.Description,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                CreatedDate = book.CreatedDate,
                IsActive = book.IsActive,
                CategoryId = book.CategoryId,
                CategoryName = book.Category?.CategoryName
            };
        }

        public static IEnumerable<BookListDto> ToBookListDTOs(this IEnumerable<Book> books)
        {
            return books.Select(ToBookListDTO);
        }
    }
} 