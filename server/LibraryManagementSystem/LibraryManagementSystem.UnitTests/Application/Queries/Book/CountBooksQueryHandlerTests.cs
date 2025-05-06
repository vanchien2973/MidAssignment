using LibraryManagementSystem.Application.Handlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Book;

public class CountBooksQueryHandlerTests
{
    private Mock<IBookRepository> _mockBookRepository;
    private CountBooksQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _handler = new CountBooksQueryHandler(_mockBookRepository.Object);
    }

    [Test]
    public async Task Handle_ReturnsCorrectCount()
    {
        // Arrange
        int expectedCount = 42;
        var query = new CountBooksQuery();
        
        _mockBookRepository.Setup(repo => repo.CountAsync())
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockBookRepository.Verify(repo => repo.CountAsync(), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryReturnsZero_ReturnsZero()
    {
        // Arrange
        int expectedCount = 0;
        var query = new CountBooksQuery();
        
        _mockBookRepository.Setup(repo => repo.CountAsync())
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockBookRepository.Verify(repo => repo.CountAsync(), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var query = new CountBooksQuery();
        var expectedException = new Exception("Database error");
        
        _mockBookRepository.Setup(repo => repo.CountAsync())
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        _mockBookRepository.Verify(repo => repo.CountAsync(), Times.Once);
    }
} 