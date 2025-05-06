using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Handlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using LibraryManagementSystem.Domain.Entities;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Book;

public class GetAvailableBooksQueryHandlerTests
{
    private Mock<IBookRepository> _mockBookRepository;
    private GetAvailableBooksQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _handler = new GetAvailableBooksQueryHandler(_mockBookRepository.Object);
    }

    [Test]
    public async Task Handle_ReturnsAvailableBooks()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetAvailableBooksQuery(pageNumber, pageSize);
        
        var books = new List<LibraryManagementSystem.Domain.Entities.Book>
        {
            new() { 
                BookId = Guid.NewGuid(), 
                Title = "Available Book 1", 
                Author = "Author 1",
                ISBN = "ISBN-1",
                TotalCopies = 5,
                AvailableCopies = 3
            },
            new() { 
                BookId = Guid.NewGuid(), 
                Title = "Available Book 2", 
                Author = "Author 2",
                ISBN = "ISBN-2",
                TotalCopies = 10,
                AvailableCopies = 7
            }
        };
        
        _mockBookRepository.Setup(repo => repo.GetAvailableBooksAsync(pageNumber, pageSize))
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
        
        _mockBookRepository.Verify(repo => repo.GetAvailableBooksAsync(pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNoAvailableBooks_ReturnsEmptyList()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetAvailableBooksQuery(pageNumber, pageSize);
        
        var emptyBooks = new List<LibraryManagementSystem.Domain.Entities.Book>();
        
        _mockBookRepository.Setup(repo => repo.GetAvailableBooksAsync(pageNumber, pageSize))
            .ReturnsAsync(emptyBooks);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockBookRepository.Verify(repo => repo.GetAvailableBooksAsync(pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetAvailableBooksQuery(pageNumber, pageSize);
        
        var expectedException = new Exception("Database error");
        
        _mockBookRepository.Setup(repo => repo.GetAvailableBooksAsync(pageNumber, pageSize))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        
        _mockBookRepository.Verify(repo => repo.GetAvailableBooksAsync(pageNumber, pageSize), Times.Once);
    }
} 