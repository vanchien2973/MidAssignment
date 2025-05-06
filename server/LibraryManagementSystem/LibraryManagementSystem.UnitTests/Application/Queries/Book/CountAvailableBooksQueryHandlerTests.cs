using LibraryManagementSystem.Application.Handlers.QueryHandlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Book;

public class CountAvailableBooksQueryHandlerTests
{
    private Mock<IBookRepository> _mockBookRepository;
    private CountAvailableBooksQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _handler = new CountAvailableBooksQueryHandler(_mockBookRepository.Object);
    }

    [Test]
    public async Task Handle_ReturnsCorrectCount()
    {
        // Arrange
        int expectedCount = 10;
        var query = new CountAvailableBooksQuery();
        
        _mockBookRepository.Setup(repo => repo.CountAvailableBooksAsync())
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockBookRepository.Verify(repo => repo.CountAvailableBooksAsync(), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var query = new CountAvailableBooksQuery();
        var expectedException = new Exception("Database error");
        
        _mockBookRepository.Setup(repo => repo.CountAvailableBooksAsync())
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        _mockBookRepository.Verify(repo => repo.CountAvailableBooksAsync(), Times.Once);
    }
} 