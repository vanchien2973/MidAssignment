using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Book;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.Book
{
    public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, IEnumerable<BookListDto>>
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<GetAllBooksQueryHandler> _logger;

        public GetAllBooksQueryHandler(
            IBookRepository bookRepository,
            ILogger<GetAllBooksQueryHandler> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<BookListDto>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var books = await _bookRepository.GetAllAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SortBy,
                    request.SortOrder);

                return books.ToBookListDTOs();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books");
                return Enumerable.Empty<BookListDto>();
            }
        }
    }
} 