using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Book;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.Book
{
    public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDetailsDto>
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<GetBookByIdQueryHandler> _logger;

        public GetBookByIdQueryHandler(
            IBookRepository bookRepository,
            ILogger<GetBookByIdQueryHandler> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<BookDetailsDto> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var book = await _bookRepository.GetByIdAsync(request.BookId);
                
                if (book == null)
                {
                    _logger.LogWarning("Book with ID {BookId} not found", request.BookId);
                    return null;
                }

                return book.ToBookDetailsDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book with ID {BookId}", request.BookId);
                return null;
            }
        }
    }
} 