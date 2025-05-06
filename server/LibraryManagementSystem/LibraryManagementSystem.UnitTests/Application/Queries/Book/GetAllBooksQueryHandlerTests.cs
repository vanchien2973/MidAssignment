using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Book;

public class GetAllBooksQueryHandlerTests
{
    private Mock<IBookRepository> _mockBookRepository;
    private Mock<ILogger<GetAllBooksQueryHandler>> _mockLogger;
    private GetAllBooksQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockLogger = new Mock<ILogger<GetAllBooksQueryHandler>>();
        _handler = new GetAllBooksQueryHandler(_mockBookRepository.Object, _mockLogger.Object);
    }

    [Test]
    public async Task Handle_WithValidParameters_ReturnsBooks()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string sortBy = "Title";
        string sortOrder = "asc";
        
        var query = new GetAllBooksQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        
        var books = new List<LibraryManagementSystem.Domain.Entities.Book>
        {
            new() { 
                BookId = Guid.NewGuid(), 
                Title = "Book 1", 
                Author = "Author 1",
                ISBN = "ISBN-1",
                TotalCopies = 5,
                AvailableCopies = 3
            },
            new() { 
                BookId = Guid.NewGuid(), 
                Title = "Book 2", 
                Author = "Author 2",
                ISBN = "ISBN-2",
                TotalCopies = 10,
                AvailableCopies = 7
            }
        };
        
        _mockBookRepository.Setup(repo => repo.GetAllAsync(
                pageNumber, pageSize, sortBy, sortOrder))
            .ReturnsAsync(books);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(books.Count));
        
        var resultList = result.ToList();
        Assert.That(resultList[0].BookId, Is.EqualTo(books[0].BookId));
        Assert.That(resultList[0].Title, Is.EqualTo(books[0].Title));
        Assert.That(resultList[1].BookId, Is.EqualTo(books[1].BookId));
        Assert.That(resultList[1].Title, Is.EqualTo(books[1].Title));
        
        _mockBookRepository.Verify(repo => repo.GetAllAsync(
            pageNumber, pageSize, sortBy, sortOrder), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithEmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string sortBy = "Title";
        string sortOrder = "asc";
        
        var query = new GetAllBooksQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        
        var emptyBooks = new List<LibraryManagementSystem.Domain.Entities.Book>();
        
        _mockBookRepository.Setup(repo => repo.GetAllAsync(
                pageNumber, pageSize, sortBy, sortOrder))
            .ReturnsAsync(emptyBooks);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockBookRepository.Verify(repo => repo.GetAllAsync(
            pageNumber, pageSize, sortBy, sortOrder), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_LogsErrorAndReturnsEmptyList()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string sortBy = "Title";
        string sortOrder = "asc";
        
        var query = new GetAllBooksQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        
        var expectedException = new Exception("Database error");
        
        _mockBookRepository.Setup(repo => repo.GetAllAsync(
                pageNumber, pageSize, sortBy, sortOrder))
            .ThrowsAsync(expectedException);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockBookRepository.Verify(repo => repo.GetAllAsync(
            pageNumber, pageSize, sortBy, sortOrder), Times.Once);
            
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
