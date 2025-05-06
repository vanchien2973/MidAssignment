using LibraryManagementSystem.Application.DTOs.Category;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Category;
using LibraryManagementSystem.Domain.Entities;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Category;

public class GetAllCategoriesQueryHandlerTests
{
    private Mock<ICategoryRepository> _mockCategoryRepository;
    private GetAllCategoriesQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _handler = new GetAllCategoriesQueryHandler(_mockCategoryRepository.Object);
    }

    [Test]
    public async Task Handle_WithValidParameters_ReturnsCategories()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string sortBy = "Name";
        string sortOrder = "asc";
        string searchTerm = null;
        
        var query = new GetAllCategoriesQuery(pageNumber, pageSize, sortBy, sortOrder, searchTerm);
        
        var categories = new List<LibraryManagementSystem.Domain.Entities.Category>
        {
            new() { CategoryId = Guid.NewGuid(), CategoryName = "Fiction", Description = "Fiction books" },
            new() { CategoryId = Guid.NewGuid(), CategoryName = "Non-Fiction", Description = "Non-fiction books" }
        };
        
        _mockCategoryRepository.Setup(repo => repo.GetAllAsync(
                pageNumber, pageSize, sortBy, sortOrder, searchTerm))
            .ReturnsAsync(categories);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(categories.Count));
        
        var resultList = result.ToList();
        Assert.That(resultList[0].CategoryId, Is.EqualTo(categories[0].CategoryId));
        Assert.That(resultList[0].CategoryName, Is.EqualTo(categories[0].CategoryName));
        Assert.That(resultList[1].CategoryId, Is.EqualTo(categories[1].CategoryId));
        Assert.That(resultList[1].CategoryName, Is.EqualTo(categories[1].CategoryName));
        
        _mockCategoryRepository.Verify(repo => repo.GetAllAsync(
            pageNumber, pageSize, sortBy, sortOrder, searchTerm), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithSearchTerm_ReturnsFilteredCategories()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string sortBy = "Name";
        string sortOrder = "asc";
        string searchTerm = "Fiction";
        
        var query = new GetAllCategoriesQuery(pageNumber, pageSize, sortBy, sortOrder, searchTerm);
        
        var categories = new List<LibraryManagementSystem.Domain.Entities.Category>
        {
            new() { CategoryId = Guid.NewGuid(), CategoryName = "Fiction", Description = "Fiction books" },
            new() { CategoryId = Guid.NewGuid(), CategoryName = "Science Fiction", Description = "Sci-fi books" }
        };
        
        _mockCategoryRepository.Setup(repo => repo.GetAllAsync(
                pageNumber, pageSize, sortBy, sortOrder, searchTerm))
            .ReturnsAsync(categories);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(categories.Count));
        
        var resultList = result.ToList();
        Assert.That(resultList[0].CategoryId, Is.EqualTo(categories[0].CategoryId));
        Assert.That(resultList[0].CategoryName, Is.EqualTo(categories[0].CategoryName));
        Assert.That(resultList[1].CategoryId, Is.EqualTo(categories[1].CategoryId));
        Assert.That(resultList[1].CategoryName, Is.EqualTo(categories[1].CategoryName));
        
        _mockCategoryRepository.Verify(repo => repo.GetAllAsync(
            pageNumber, pageSize, sortBy, sortOrder, searchTerm), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithEmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string sortBy = "Name";
        string sortOrder = "asc";
        string searchTerm = null;
        
        var query = new GetAllCategoriesQuery(pageNumber, pageSize, sortBy, sortOrder, searchTerm);
        
        var emptyCategories = new List<LibraryManagementSystem.Domain.Entities.Category>();
        
        _mockCategoryRepository.Setup(repo => repo.GetAllAsync(
                pageNumber, pageSize, sortBy, sortOrder, searchTerm))
            .ReturnsAsync(emptyCategories);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockCategoryRepository.Verify(repo => repo.GetAllAsync(
            pageNumber, pageSize, sortBy, sortOrder, searchTerm), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string sortBy = "Name";
        string sortOrder = "asc";
        string searchTerm = null;
        
        var query = new GetAllCategoriesQuery(pageNumber, pageSize, sortBy, sortOrder, searchTerm);
        
        var expectedException = new Exception("Database error");
        
        _mockCategoryRepository.Setup(repo => repo.GetAllAsync(
                pageNumber, pageSize, sortBy, sortOrder, searchTerm))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        
        _mockCategoryRepository.Verify(repo => repo.GetAllAsync(
            pageNumber, pageSize, sortBy, sortOrder, searchTerm), Times.Once);
    }
}
