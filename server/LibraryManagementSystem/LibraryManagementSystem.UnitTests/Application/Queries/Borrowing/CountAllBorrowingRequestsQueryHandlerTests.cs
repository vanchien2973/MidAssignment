using LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Borrowing;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Borrowing;

public class CountAllBorrowingRequestsQueryHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<CountAllBorrowingRequestsQueryHandler>> _mockLogger;
    private Mock<IBookBorrowingRequestRepository> _mockBorrowingRepository;
    private CountAllBorrowingRequestsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<CountAllBorrowingRequestsQueryHandler>>();
        _mockBorrowingRepository = new Mock<IBookBorrowingRequestRepository>();
        
        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequests).Returns(_mockBorrowingRepository.Object);
        
        _handler = new CountAllBorrowingRequestsQueryHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_ReturnsCorrectCount()
    {
        // Arrange
        int expectedCount = 10;
        var query = new CountAllBorrowingRequestsQuery();
        
        _mockBorrowingRepository.Setup(repo => repo.CountAsync())
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockBorrowingRepository.Verify(repo => repo.CountAsync(), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryReturnsZero_ReturnsZero()
    {
        // Arrange
        int expectedCount = 0;
        var query = new CountAllBorrowingRequestsQuery();
        
        _mockBorrowingRepository.Setup(repo => repo.CountAsync())
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockBorrowingRepository.Verify(repo => repo.CountAsync(), Times.Once);
    }
    
    [Test]
    public void Handle_WhenRepositoryThrowsException_LogsErrorAndPropagatesException()
    {
        // Arrange
        var query = new CountAllBorrowingRequestsQuery();
        var expectedException = new Exception("Database error");
        
        _mockBorrowingRepository.Setup(repo => repo.CountAsync())
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        _mockBorrowingRepository.Verify(repo => repo.CountAsync(), Times.Once);
        
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