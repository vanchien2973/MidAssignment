using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Book;

public class GetBookByIdQueryHandlerTests
{
    private Mock<IBookRepository> _mockBookRepository;
    private Mock<ILogger<GetBookByIdQueryHandler>> _mockLogger;
    private GetBookByIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockLogger = new Mock<ILogger<GetBookByIdQueryHandler>>();
        _handler = new GetBookByIdQueryHandler(_mockBookRepository.Object, _mockLogger.Object);
    }

    [Test]
    public async Task Handle_WithExistingBookId_ReturnsBook()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new GetBookByIdQuery(bookId);
        
        var book = new LibraryManagementSystem.Domain.Entities.Book
        {
            BookId = bookId,
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "1234567890",
            Description = "Test Description",
            PublishedYear = 2022,
            TotalCopies = 5,
            AvailableCopies = 3,
            CategoryId = Guid.NewGuid()
        };
        
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.BookId, Is.EqualTo(book.BookId));
        Assert.That(result.Title, Is.EqualTo(book.Title));
        Assert.That(result.Author, Is.EqualTo(book.Author));
        Assert.That(result.ISBN, Is.EqualTo(book.ISBN));
        Assert.That(result.Description, Is.EqualTo(book.Description));
        
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNonExistentBookId_ReturnsNull()
    {
        // Arrange
        var nonExistentBookId = Guid.NewGuid();
        var query = new GetBookByIdQuery(nonExistentBookId);
        
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(nonExistentBookId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Book)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Null);
        
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(nonExistentBookId), Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_LogsErrorAndReturnsNull()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new GetBookByIdQuery(bookId);
        var expectedException = new Exception("Database error");
        
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ThrowsAsync(expectedException);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Null);
        
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
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
