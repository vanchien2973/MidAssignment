using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Borrowing;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing;

public class CountAllBorrowingRequestsQueryHandler : IRequestHandler<CountAllBorrowingRequestsQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CountAllBorrowingRequestsQueryHandler> _logger;

    public CountAllBorrowingRequestsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<CountAllBorrowingRequestsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> Handle(CountAllBorrowingRequestsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var count = await _unitOfWork.BookBorrowingRequests.CountAsync();
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting all borrowing requests");
            throw;
        }
    }
} 