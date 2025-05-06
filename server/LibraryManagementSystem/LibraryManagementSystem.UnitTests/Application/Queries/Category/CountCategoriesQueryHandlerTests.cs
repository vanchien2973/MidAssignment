using LibraryManagementSystem.Application.Handlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Category;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Category;

public class CountCategoriesQueryHandlerTests
{
    private Mock<ICategoryRepository> _mockCategoryRepository;
    private CountCategoriesQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _handler = new CountCategoriesQueryHandler(_mockCategoryRepository.Object);
    }

    [Test]
    public async Task Handle_WithNoSearchTerm_ReturnsCorrectCount()
    {
        // Arrange
        int expectedCount = 10;
        string searchTerm = null;
        var query = new CountCategoriesQuery(searchTerm);
        
        _mockCategoryRepository.Setup(repo => repo.CountAsync(searchTerm))
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockCategoryRepository.Verify(repo => repo.CountAsync(searchTerm), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithSearchTerm_ReturnsCorrectCount()
    {
        // Arrange
        int expectedCount = 5;
        string searchTerm = "fiction";
        var query = new CountCategoriesQuery(searchTerm);
        
        _mockCategoryRepository.Setup(repo => repo.CountAsync(searchTerm))
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockCategoryRepository.Verify(repo => repo.CountAsync(searchTerm), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNoMatches_ReturnsZero()
    {
        // Arrange
        int expectedCount = 0;
        string searchTerm = "nonexistent";
        var query = new CountCategoriesQuery(searchTerm);
        
        _mockCategoryRepository.Setup(repo => repo.CountAsync(searchTerm))
            .ReturnsAsync(expectedCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
        _mockCategoryRepository.Verify(repo => repo.CountAsync(searchTerm), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        string searchTerm = "fiction";
        var query = new CountCategoriesQuery(searchTerm);
        var expectedException = new Exception("Database error");
        
        _mockCategoryRepository.Setup(repo => repo.CountAsync(searchTerm))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        _mockCategoryRepository.Verify(repo => repo.CountAsync(searchTerm), Times.Once);
    }
} 