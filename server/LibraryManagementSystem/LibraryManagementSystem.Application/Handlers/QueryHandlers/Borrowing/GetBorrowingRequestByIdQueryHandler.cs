using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Borrowing;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing
{
    public class GetBorrowingRequestByIdQueryHandler : IRequestHandler<GetBorrowingRequestByIdQuery, BookBorrowingRequestDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetBorrowingRequestByIdQueryHandler> _logger;

        public GetBorrowingRequestByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetBorrowingRequestByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BookBorrowingRequestDto> Handle(GetBorrowingRequestByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var borrowingRequest = await _unitOfWork.BookBorrowingRequests.GetByIdAsync(request.RequestId);
                
                if (borrowingRequest == null)
                {
                    return null;
                }

                // Map entity to DTO
                return BorrowingMapper.ToDto(borrowingRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrowing request {RequestId}", request.RequestId);
                throw;
            }
        }
    }
} 