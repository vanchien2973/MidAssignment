using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Borrowing;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Borrowing;

public class GetPendingBorrowingRequestsQueryHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<GetPendingBorrowingRequestsQueryHandler>> _mockLogger;
    private Mock<IBookBorrowingRequestRepository> _mockBorrowingRepository;
    private GetPendingBorrowingRequestsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<GetPendingBorrowingRequestsQueryHandler>>();
        _mockBorrowingRepository = new Mock<IBookBorrowingRequestRepository>();
        
        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequests).Returns(_mockBorrowingRepository.Object);
        
        _handler = new GetPendingBorrowingRequestsQueryHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithPendingRequests_ReturnsPendingRequests()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        bool includeAllStatuses = false;
        
        var query = new GetPendingBorrowingRequestsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeAllStatuses = includeAllStatuses
        };
        
        var pendingRequests = new List<BookBorrowingRequest>
        {
            new() {
                RequestId = Guid.NewGuid(),
                RequestorId = 201,
                RequestDate = DateTime.UtcNow.AddDays(-1),
                Status = BorrowingRequestStatus.Waiting
            },
            new() {
                RequestId = Guid.NewGuid(),
                RequestorId = 202,
                RequestDate = DateTime.UtcNow.AddDays(-2),
                Status = BorrowingRequestStatus.Waiting
            }
        };
        
        _mockBorrowingRepository.Setup(repo => repo.GetByStatusAsync(BorrowingRequestStatus.Waiting, pageNumber, pageSize))
            .ReturnsAsync(pendingRequests);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(pendingRequests.Count));
        
        var resultList = result.ToList();
        Assert.That(resultList[0].RequestId, Is.EqualTo(pendingRequests[0].RequestId));
        Assert.That(resultList[0].Status, Is.EqualTo(pendingRequests[0].Status));
        Assert.That(resultList[1].RequestId, Is.EqualTo(pendingRequests[1].RequestId));
        Assert.That(resultList[1].Status, Is.EqualTo(pendingRequests[1].Status));
        
        _mockBorrowingRepository.Verify(repo => repo.GetByStatusAsync(BorrowingRequestStatus.Waiting, pageNumber, pageSize), Times.Once);
        _mockBorrowingRepository.Verify(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
    
    [Test]
    public async Task Handle_WithIncludeAllStatuses_ReturnsAllRequests()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        bool includeAllStatuses = true;
        
        var query = new GetPendingBorrowingRequestsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeAllStatuses = includeAllStatuses
        };
        
        var allRequests = new List<BookBorrowingRequest>
        {
            new() {
                RequestId = Guid.NewGuid(),
                RequestorId = 201,
                RequestDate = DateTime.UtcNow.AddDays(-1),
                Status = BorrowingRequestStatus.Waiting
            },
            new() {
                RequestId = Guid.NewGuid(),
                RequestorId = 202,
                RequestDate = DateTime.UtcNow.AddDays(-2),
                Status = BorrowingRequestStatus.Approved
            },
            new() {
                RequestId = Guid.NewGuid(),
                RequestorId = 203,
                RequestDate = DateTime.UtcNow.AddDays(-3),
                Status = BorrowingRequestStatus.Rejected
            }
        };
        
        _mockBorrowingRepository.Setup(repo => repo.GetAllAsync(pageNumber, pageSize))
            .ReturnsAsync(allRequests);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(allRequests.Count));
        
        var resultList = result.ToList();
        Assert.That(resultList[0].RequestId, Is.EqualTo(allRequests[0].RequestId));
        Assert.That(resultList[0].Status, Is.EqualTo(allRequests[0].Status));
        Assert.That(resultList[1].RequestId, Is.EqualTo(allRequests[1].RequestId));
        Assert.That(resultList[1].Status, Is.EqualTo(allRequests[1].Status));
        Assert.That(resultList[2].RequestId, Is.EqualTo(allRequests[2].RequestId));
        Assert.That(resultList[2].Status, Is.EqualTo(allRequests[2].Status));
        
        _mockBorrowingRepository.Verify(repo => repo.GetAllAsync(pageNumber, pageSize), Times.Once);
        _mockBorrowingRepository.Verify(repo => repo.GetByStatusAsync(It.IsAny<BorrowingRequestStatus>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
    
    [Test]
    public async Task Handle_WithNoPendingRequests_ReturnsEmptyList()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        bool includeAllStatuses = false;
        
        var query = new GetPendingBorrowingRequestsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeAllStatuses = includeAllStatuses
        };
        
        var emptyRequests = new List<BookBorrowingRequest>();
        
        _mockBorrowingRepository.Setup(repo => repo.GetByStatusAsync(BorrowingRequestStatus.Waiting, pageNumber, pageSize))
            .ReturnsAsync(emptyRequests);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockBorrowingRepository.Verify(repo => repo.GetByStatusAsync(BorrowingRequestStatus.Waiting, pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public void Handle_WhenRepositoryThrowsException_LogsErrorAndPropagatesException()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        bool includeAllStatuses = false;
        
        var query = new GetPendingBorrowingRequestsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeAllStatuses = includeAllStatuses
        };
        
        var expectedException = new Exception("Database error");
        
        _mockBorrowingRepository.Setup(repo => repo.GetByStatusAsync(BorrowingRequestStatus.Waiting, pageNumber, pageSize))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        
        _mockBorrowingRepository.Verify(repo => repo.GetByStatusAsync(BorrowingRequestStatus.Waiting, pageNumber, pageSize), Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
} 