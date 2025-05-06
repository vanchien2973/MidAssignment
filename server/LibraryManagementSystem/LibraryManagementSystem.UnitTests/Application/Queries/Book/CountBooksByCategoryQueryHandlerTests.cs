using LibraryManagementSystem.Application.Handlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Book;

public class CountBooksByCategoryQueryHandlerTests
{
    private Mock<IBookRepository> _mockBookRepository;
    private CountBooksByCategoryQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _handler = new CountBooksByCategoryQueryHandler(_mockBookRepository.Object);
    }

    [Test]
    public async Task Handle_WithValidCategoryId_ReturnsCorrectCount()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        int expectedCount = 5;
        var query = new CountBooksByCategoryQuery(categoryId);
        
        _mockBookRepository.Setup(repo => repo.CountByCategoryAsync(categoryId))
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockBookRepository.Verify(repo => repo.CountByCategoryAsync(categoryId), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNonExistentCategoryId_ReturnsZero()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();
        int expectedCount = 0;
        var query = new CountBooksByCategoryQuery(nonExistentCategoryId);
        
        _mockBookRepository.Setup(repo => repo.CountByCategoryAsync(nonExistentCategoryId))
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockBookRepository.Verify(repo => repo.CountByCategoryAsync(nonExistentCategoryId), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new CountBooksByCategoryQuery(categoryId);
        var expectedException = new Exception("Database error");
        
        _mockBookRepository.Setup(repo => repo.CountByCategoryAsync(categoryId))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        _mockBookRepository.Verify(repo => repo.CountByCategoryAsync(categoryId), Times.Once);
    }
} 