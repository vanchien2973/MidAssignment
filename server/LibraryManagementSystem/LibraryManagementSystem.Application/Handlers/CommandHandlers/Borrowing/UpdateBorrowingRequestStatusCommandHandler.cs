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
    public class UpdateBorrowingRequestStatusCommandHandler : IRequestHandler<UpdateBorrowingRequestStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateBorrowingRequestStatusCommandHandler> _logger;

        public UpdateBorrowingRequestStatusCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateBorrowingRequestStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(UpdateBorrowingRequestStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var borrowingRequest = await _unitOfWork.BookBorrowingRequests.GetByIdAsync(request.RequestId);
                if (borrowingRequest == null || borrowingRequest.Status != BorrowingRequestStatus.Waiting)
                {
                    return false;
                }

                borrowingRequest.Status = request.Status;
                borrowingRequest.ApproverId = request.ApproverId;
                borrowingRequest.ApprovalDate = DateTime.UtcNow;
                borrowingRequest.Notes = string.IsNullOrEmpty(borrowingRequest.Notes) 
                    ? request.Notes 
                    : $"{borrowingRequest.Notes}\n{request.Notes}";

                // If approved, set due dates for books (default to 14 days)
                if (request.Status == BorrowingRequestStatus.Approved)
                {
                    // Sử dụng DueDays từ request nếu có, nếu không sử dụng mặc định 14 ngày
                    int dueDays = request.DueDays ?? 14;
                    var dueDate = DateTime.UtcNow.AddDays(dueDays);

                    foreach (var detail in borrowingRequest.RequestDetails)
                    {
                        detail.DueDate = dueDate;

                        // Update available copies of the book
                        var book = await _unitOfWork.Books.GetByIdAsync(detail.BookId);
                        if (book != null && book.AvailableCopies > 0)
                        {
                            book.AvailableCopies--;
                            await _unitOfWork.Books.UpdateAsync(book);
                        }
                    }
                }

                await _unitOfWork.BookBorrowingRequests.UpdateAsync(borrowingRequest);
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Borrowing request {RequestId} status updated to {Status} by user {ApproverId}", 
                    request.RequestId, request.Status, request.ApproverId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating borrowing request status {RequestId}", request.RequestId);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                return false;
            }
        }
    }
} 