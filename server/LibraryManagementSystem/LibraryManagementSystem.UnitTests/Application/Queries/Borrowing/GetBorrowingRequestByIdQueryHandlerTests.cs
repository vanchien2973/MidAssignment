using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Borrowing;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Borrowing;

public class GetBorrowingRequestByIdQueryHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<GetBorrowingRequestByIdQueryHandler>> _mockLogger;
    private Mock<IBookBorrowingRequestRepository> _mockBorrowingRepository;
    private GetBorrowingRequestByIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<GetBorrowingRequestByIdQueryHandler>>();
        _mockBorrowingRepository = new Mock<IBookBorrowingRequestRepository>();
        
        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequests).Returns(_mockBorrowingRepository.Object);
        
        _handler = new GetBorrowingRequestByIdQueryHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithExistingRequestId_ReturnsRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetBorrowingRequestByIdQuery { RequestId = requestId };
        
        var borrowingRequest = new BookBorrowingRequest
        {
            RequestId = requestId,
            RequestorId = 3,
            RequestDate = DateTime.UtcNow.AddDays(-5),
            Status = LibraryManagementSystem.Domain.Enums.BorrowingRequestStatus.Approved,
            ApprovalDate = DateTime.UtcNow.AddDays(-3),
            RequestDetails = new List<BookBorrowingRequestDetail>()
        };
        
        _mockBorrowingRepository.Setup(repo => repo.GetByIdAsync(requestId))
            .ReturnsAsync(borrowingRequest);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RequestId, Is.EqualTo(borrowingRequest.RequestId));
        Assert.That(result.RequestorId, Is.EqualTo(borrowingRequest.RequestorId));
        Assert.That(result.Status, Is.EqualTo(borrowingRequest.Status));
        
        _mockBorrowingRepository.Verify(repo => repo.GetByIdAsync(requestId), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNonExistentRequestId_ReturnsNull()
    {
        // Arrange
        var nonExistentRequestId = Guid.NewGuid();
        var query = new GetBorrowingRequestByIdQuery { RequestId = nonExistentRequestId };
        
        _mockBorrowingRepository.Setup(repo => repo.GetByIdAsync(nonExistentRequestId))
            .ReturnsAsync((BookBorrowingRequest)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Null);
        
        _mockBorrowingRepository.Verify(repo => repo.GetByIdAsync(nonExistentRequestId), Times.Once);
    }
    
    [Test]
    public void Handle_WhenRepositoryThrowsException_LogsErrorAndPropagatesException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetBorrowingRequestByIdQuery { RequestId = requestId };
        var expectedException = new Exception("Database error");
        
        _mockBorrowingRepository.Setup(repo => repo.GetByIdAsync(requestId))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        
        _mockBorrowingRepository.Verify(repo => repo.GetByIdAsync(requestId), Times.Once);
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
