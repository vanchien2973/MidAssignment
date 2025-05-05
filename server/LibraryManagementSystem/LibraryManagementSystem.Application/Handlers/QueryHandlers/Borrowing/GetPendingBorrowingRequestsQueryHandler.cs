using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Borrowing;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing
{
    public class GetPendingBorrowingRequestsQueryHandler : IRequestHandler<GetPendingBorrowingRequestsQuery, IEnumerable<BookBorrowingRequestDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetPendingBorrowingRequestsQueryHandler> _logger;

        public GetPendingBorrowingRequestsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetPendingBorrowingRequestsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<BookBorrowingRequestDto>> Handle(GetPendingBorrowingRequestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.IncludeAllStatuses)
                {
                    var allRequests = await _unitOfWork.BookBorrowingRequests.GetAllAsync(
                        request.PageNumber,
                        request.PageSize);
                    
                    if (allRequests == null || !allRequests.Any())
                    {
                        return new List<BookBorrowingRequestDto>();
                    }
                    
                    return allRequests.Select(BorrowingMapper.ToDto).ToList();
                }
                else
                {
                    var borrowingRequests = await _unitOfWork.BookBorrowingRequests.GetByStatusAsync(
                        BorrowingRequestStatus.Waiting, 
                        request.PageNumber, 
                        request.PageSize);
                    
                    if (borrowingRequests == null || !borrowingRequests.Any())
                    {
                        return new List<BookBorrowingRequestDto>();
                    }

                    // Map entities to DTOs
                    return borrowingRequests.Select(BorrowingMapper.ToDto).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrowing requests");
                throw;
            }
        }
    }
} 