using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Borrowing;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Borrowing;

public class GetOverdueBorrowingsQueryHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<GetOverdueBorrowingsQueryHandler>> _mockLogger;
    private Mock<IBookBorrowingRequestDetailRepository> _mockBorrowingDetailRepository;
    private GetOverdueBorrowingsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<GetOverdueBorrowingsQueryHandler>>();
        _mockBorrowingDetailRepository = new Mock<IBookBorrowingRequestDetailRepository>();
        
        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequestDetails).Returns(_mockBorrowingDetailRepository.Object);
        
        _handler = new GetOverdueBorrowingsQueryHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithOverdueItems_ReturnsOverdueItems()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetOverdueBorrowingsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var overdueItems = new List<BookBorrowingRequestDetail>
        {
            new() {
                DetailId = Guid.NewGuid(),
                RequestId = Guid.NewGuid(),
                BookId = Guid.NewGuid(),
                ReturnDate = null,
                DueDate = DateTime.UtcNow.AddDays(-5), // Overdue
                Status = BorrowingDetailStatus.Borrowing
            },
            new() {
                DetailId = Guid.NewGuid(),
                RequestId = Guid.NewGuid(),
                BookId = Guid.NewGuid(),
                ReturnDate = null,
                DueDate = DateTime.UtcNow.AddDays(-2), // Overdue
                Status = BorrowingDetailStatus.Borrowing
            }
        };
        
        _mockBorrowingDetailRepository.Setup(repo => repo.GetOverdueItemsAsync(pageNumber, pageSize))
            .ReturnsAsync(overdueItems);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(overdueItems.Count));
        
        var resultList = result.ToList();
        Assert.That(resultList[0].DetailId, Is.EqualTo(overdueItems[0].DetailId));
        Assert.That(resultList[0].RequestId, Is.EqualTo(overdueItems[0].RequestId));
        Assert.That(resultList[1].DetailId, Is.EqualTo(overdueItems[1].DetailId));
        Assert.That(resultList[1].RequestId, Is.EqualTo(overdueItems[1].RequestId));
        
        _mockBorrowingDetailRepository.Verify(repo => repo.GetOverdueItemsAsync(pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNoOverdueItems_ReturnsEmptyList()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetOverdueBorrowingsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var emptyItems = new List<BookBorrowingRequestDetail>();
        
        _mockBorrowingDetailRepository.Setup(repo => repo.GetOverdueItemsAsync(pageNumber, pageSize))
            .ReturnsAsync(emptyItems);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockBorrowingDetailRepository.Verify(repo => repo.GetOverdueItemsAsync(pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetOverdueBorrowingsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        _mockBorrowingDetailRepository.Setup(repo => repo.GetOverdueItemsAsync(pageNumber, pageSize))
            .ReturnsAsync((IEnumerable<BookBorrowingRequestDetail>)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockBorrowingDetailRepository.Verify(repo => repo.GetOverdueItemsAsync(pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public void Handle_WhenRepositoryThrowsException_LogsErrorAndPropagatesException()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetOverdueBorrowingsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var expectedException = new Exception("Database error");
        
        _mockBorrowingDetailRepository.Setup(repo => repo.GetOverdueItemsAsync(pageNumber, pageSize))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        
        _mockBorrowingDetailRepository.Verify(repo => repo.GetOverdueItemsAsync(pageNumber, pageSize), Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.Is<Exception>(ex => ex == expectedException),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
} 