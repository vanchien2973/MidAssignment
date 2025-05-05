using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Book
{
    public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateBookCommandHandler> _logger;

        public UpdateBookCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateBookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var book = await _unitOfWork.Books.GetByIdAsync(request.BookId);
                
                if (book == null)
                {
                    _logger.LogWarning("Book with ID {BookId} not found for update", request.BookId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }
                
                // Update book entity
                book.UpdateFrom(request);
                
                await _unitOfWork.Books.UpdateAsync(book);
                
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Book {BookTitle} updated with ID {BookId}", 
                    book.Title, book.BookId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book {BookId}", request.BookId);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                return false;
            }
        }
    }
} 