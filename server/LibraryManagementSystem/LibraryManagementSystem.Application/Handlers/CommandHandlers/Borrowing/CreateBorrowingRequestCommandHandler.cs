using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Borrowing
{
    public class CreateBorrowingRequestCommandHandler : IRequestHandler<CreateBorrowingRequestCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateBorrowingRequestCommandHandler> _logger;

        public CreateBorrowingRequestCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CreateBorrowingRequestCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateBorrowingRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Create the main borrowing request
                var borrowingRequest = new BookBorrowingRequest
                {
                    RequestId = Guid.NewGuid(),
                    RequestorId = request.RequestorId,
                    RequestDate = DateTime.UtcNow,
                    Status = BorrowingRequestStatus.Waiting,
                    Notes = request.Notes,
                    RequestDetails = new List<BookBorrowingRequestDetail>()
                };

                // Create details for each book
                foreach (var book in request.Books)
                {
                    var detail = new BookBorrowingRequestDetail
                    {
                        DetailId = Guid.NewGuid(),
                        RequestId = borrowingRequest.RequestId,
                        BookId = book.BookId,
                        Status = BorrowingDetailStatus.Borrowing
                    };

                    borrowingRequest.RequestDetails.Add(detail);
                }

                // Save the borrowing request
                await _unitOfWork.BookBorrowingRequests.CreateAsync(borrowingRequest);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Borrowing request created with ID {RequestId} for user {UserId}",
                    borrowingRequest.RequestId, request.RequestorId);

                return borrowingRequest.RequestId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating borrowing request for user {UserId}", request.RequestorId);
                
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
} 