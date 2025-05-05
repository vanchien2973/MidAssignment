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
    public class ExtendBorrowingCommandHandler : IRequestHandler<ExtendBorrowingCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExtendBorrowingCommandHandler> _logger;

        public ExtendBorrowingCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ExtendBorrowingCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(ExtendBorrowingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var detail = await _unitOfWork.BookBorrowingRequestDetails.GetByIdAsync(request.DetailId);
                
                if (detail == null || 
                    detail.Status != BorrowingDetailStatus.Borrowing || 
                    detail.ExtensionDate != null)
                {
                    return false;
                }

                // Check if new due date is valid (max 7 days extension)
                if (!detail.DueDate.HasValue || 
                    request.NewDueDate > detail.DueDate.Value.AddDays(7))
                {
                    return false;
                }

                // Update the detail
                detail.Status = BorrowingDetailStatus.Extended;
                detail.DueDate = request.NewDueDate;
                detail.ExtensionDate = DateTime.UtcNow;
                
                await _unitOfWork.BookBorrowingRequestDetails.UpdateAsync(detail);

                // Create activity log
                // Logic to record who extended the borrowing

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Book detail {DetailId} extended by user {UserId} to {NewDueDate}", 
                    request.DetailId, request.UserId, request.NewDueDate);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending book detail {DetailId}", request.DetailId);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                return false;
            }
        }
    }
} 