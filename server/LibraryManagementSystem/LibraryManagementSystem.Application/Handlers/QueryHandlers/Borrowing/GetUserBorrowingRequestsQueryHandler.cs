using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Borrowing;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing
{
    public class GetUserBorrowingRequestsQueryHandler : IRequestHandler<GetUserBorrowingRequestsQuery, IEnumerable<BookBorrowingRequestDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetUserBorrowingRequestsQueryHandler> _logger;

        public GetUserBorrowingRequestsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetUserBorrowingRequestsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<BookBorrowingRequestDto>> Handle(GetUserBorrowingRequestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var borrowingRequests = await _unitOfWork.BookBorrowingRequests.GetByUserIdAsync(
                    request.UserId, 
                    request.PageNumber, 
                    request.PageSize);
                
                if (borrowingRequests == null || !borrowingRequests.Any())
                {
                    return new List<BookBorrowingRequestDto>();
                }

                // Map entities to DTOs 
                return borrowingRequests.Select(BorrowingMapper.ToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrowing requests for user {UserId}", request.UserId);
                throw;
            }
        }
    }
} 