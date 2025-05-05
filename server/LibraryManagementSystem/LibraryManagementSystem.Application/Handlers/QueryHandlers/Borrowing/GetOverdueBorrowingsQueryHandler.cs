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
    public class GetOverdueBorrowingsQueryHandler : IRequestHandler<GetOverdueBorrowingsQuery, IEnumerable<BookBorrowingRequestDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetOverdueBorrowingsQueryHandler> _logger;

        public GetOverdueBorrowingsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetOverdueBorrowingsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<BookBorrowingRequestDetailDto>> Handle(GetOverdueBorrowingsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var overdueItems = await _unitOfWork.BookBorrowingRequestDetails.GetOverdueItemsAsync(
                    request.PageNumber, 
                    request.PageSize);
                
                if (overdueItems == null || !overdueItems.Any())
                {
                    return new List<BookBorrowingRequestDetailDto>();
                }

                // Map entities to DTOs
                return overdueItems.Select(BorrowingMapper.ToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue borrowings");
                throw;
            }
        }
    }
} 