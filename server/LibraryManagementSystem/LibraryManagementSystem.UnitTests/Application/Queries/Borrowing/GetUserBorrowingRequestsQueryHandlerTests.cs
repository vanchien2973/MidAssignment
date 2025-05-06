using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Borrowing;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Borrowing;

public class GetUserBorrowingRequestsQueryHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<GetUserBorrowingRequestsQueryHandler>> _mockLogger;
    private Mock<IBookBorrowingRequestRepository> _mockBorrowingRepository;
    private GetUserBorrowingRequestsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<GetUserBorrowingRequestsQueryHandler>>();
        _mockBorrowingRepository = new Mock<IBookBorrowingRequestRepository>();
        
        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequests).Returns(_mockBorrowingRepository.Object);
        
        _handler = new GetUserBorrowingRequestsQueryHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithExistingBorrowingRequests_ReturnsRequests()
    {
        // Arrange
        int userId = 1;
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetUserBorrowingRequestsQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var borrowingRequests = new List<BookBorrowingRequest>
        {
            new() {
                RequestId = Guid.NewGuid(),
                RequestorId = userId,
                RequestDate = DateTime.UtcNow.AddDays(-5),
                Status = BorrowingRequestStatus.Approved
            },
            new() {
                RequestId = Guid.NewGuid(),
                RequestorId = userId,
                RequestDate = DateTime.UtcNow.AddDays(-2),
                Status = BorrowingRequestStatus.Waiting
            }
        };
        
        _mockBorrowingRepository.Setup(repo => repo.GetByUserIdAsync(userId, pageNumber, pageSize))
            .ReturnsAsync(borrowingRequests);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(borrowingRequests.Count));
        
        var resultList = result.ToList();
        Assert.That(resultList[0].RequestId, Is.EqualTo(borrowingRequests[0].RequestId));
        Assert.That(resultList[0].RequestorId, Is.EqualTo(borrowingRequests[0].RequestorId));
        Assert.That(resultList[0].Status, Is.EqualTo(borrowingRequests[0].Status));
        
        Assert.That(resultList[1].RequestId, Is.EqualTo(borrowingRequests[1].RequestId));
        Assert.That(resultList[1].RequestorId, Is.EqualTo(borrowingRequests[1].RequestorId));
        Assert.That(resultList[1].Status, Is.EqualTo(borrowingRequests[1].Status));
        
        _mockBorrowingRepository.Verify(repo => repo.GetByUserIdAsync(userId, pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNoRequests_ReturnsEmptyList()
    {
        // Arrange
        int userId = 999; // User with no borrowing requests
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetUserBorrowingRequestsQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var emptyRequests = new List<BookBorrowingRequest>();
        
        _mockBorrowingRepository.Setup(repo => repo.GetByUserIdAsync(userId, pageNumber, pageSize))
            .ReturnsAsync(emptyRequests);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockBorrowingRepository.Verify(repo => repo.GetByUserIdAsync(userId, pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        int userId = 1;
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetUserBorrowingRequestsQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        _mockBorrowingRepository.Setup(repo => repo.GetByUserIdAsync(userId, pageNumber, pageSize))
            .ReturnsAsync((IEnumerable<BookBorrowingRequest>)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockBorrowingRepository.Verify(repo => repo.GetByUserIdAsync(userId, pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public void Handle_WhenRepositoryThrowsException_LogsErrorAndPropagatesException()
    {
        // Arrange
        int userId = 1;
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetUserBorrowingRequestsQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var expectedException = new Exception("Database error");
        
        _mockBorrowingRepository.Setup(repo => repo.GetByUserIdAsync(userId, pageNumber, pageSize))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        
        _mockBorrowingRepository.Verify(repo => repo.GetByUserIdAsync(userId, pageNumber, pageSize), Times.Once);
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