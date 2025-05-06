using LibraryManagementSystem.Application.DTOs.Category;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Category;
using LibraryManagementSystem.Domain.Entities;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Category;

public class GetCategoryByIdQueryHandlerTests
{
    private Mock<ICategoryRepository> _mockCategoryRepository;
    private GetCategoryByIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _handler = new GetCategoryByIdQueryHandler(_mockCategoryRepository.Object);
    }

    [Test]
    public async Task Handle_WithExistingCategoryId_ReturnsCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery(categoryId);
        
        var category = new LibraryManagementSystem.Domain.Entities.Category
        {
            CategoryId = categoryId,
            CategoryName = "Fiction",
            Description = "Fiction books"
        };
        
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CategoryId, Is.EqualTo(category.CategoryId));
        Assert.That(result.CategoryName, Is.EqualTo(category.CategoryName));
        Assert.That(result.Description, Is.EqualTo(category.Description));
        
        _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNonExistentCategoryId_ReturnsNull()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery(nonExistentCategoryId);
        
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(nonExistentCategoryId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Category)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Null);
        
        _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(nonExistentCategoryId), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery(categoryId);
        var expectedException = new Exception("Database error");
        
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        
        _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
    }
}
