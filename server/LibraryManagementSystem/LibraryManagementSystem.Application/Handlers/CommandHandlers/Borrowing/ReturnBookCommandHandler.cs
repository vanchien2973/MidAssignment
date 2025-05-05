using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Borrowing
{
    public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReturnBookCommandHandler> _logger;

        public ReturnBookCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ReturnBookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var detail = await _unitOfWork.BookBorrowingRequestDetails.GetByIdAsync(request.DetailId);
                
                if (detail == null || 
                   (detail.Status != BorrowingDetailStatus.Borrowing && 
                    detail.Status != BorrowingDetailStatus.Extended))
                {
                    return false;
                }

                // Update the detail
                detail.Status = BorrowingDetailStatus.Returned;
                detail.ReturnDate = DateTime.UtcNow;
                
                await _unitOfWork.BookBorrowingRequestDetails.UpdateAsync(detail);

                // Increase the available copies
                var book = await _unitOfWork.Books.GetByIdAsync(detail.BookId);
                if (book != null)
                {
                    book.AvailableCopies++;
                    await _unitOfWork.Books.UpdateAsync(book);
                }

                // Create activity log
                // Logic to record who returned the book

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Book detail {DetailId} returned by user {UserId}", 
                    request.DetailId, request.UserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning book detail {DetailId}", request.DetailId);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                return false;
            }
        }
    }
} 