using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Book
{
    public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteBookCommandHandler> _logger;

        public DeleteBookCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteBookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var book = await _unitOfWork.Books.GetByIdAsync(request.BookId);
                
                if (book == null)
                {
                    _logger.LogWarning("Book with ID {BookId} not found for deletion", request.BookId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Check if there are any active borrowing records first
                var hasBorrowings = await _unitOfWork.BookBorrowingRequestDetails.HasActiveBorrowingsForBookAsync(request.BookId);
                
                if (hasBorrowings)
                {
                    _logger.LogWarning("Cannot delete book {BookId} as it has active borrowings", request.BookId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }
                
                await _unitOfWork.Books.DeleteAsync(request.BookId);
                
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Book with ID {BookId} was deleted", request.BookId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book {BookId}", request.BookId);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                return false;
            }
        }
    }
} 