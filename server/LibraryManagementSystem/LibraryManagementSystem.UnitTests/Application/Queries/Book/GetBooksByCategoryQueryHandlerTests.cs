using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Handlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using LibraryManagementSystem.Domain.Entities;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Book;

public class GetBooksByCategoryQueryHandlerTests
{
    private Mock<IBookRepository> _mockBookRepository;
    private GetBooksByCategoryQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _handler = new GetBooksByCategoryQueryHandler(_mockBookRepository.Object);
    }

    [Test]
    public async Task Handle_WithValidCategoryId_ReturnsBooks()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetBooksByCategoryQuery(categoryId, pageNumber, pageSize);
        
        var books = new List<LibraryManagementSystem.Domain.Entities.Book>
        {
            new() { 
                BookId = Guid.NewGuid(), 
                Title = "Book 1", 
                Author = "Author 1",
                CategoryId = categoryId
            },
            new() { 
                BookId = Guid.NewGuid(), 
                Title = "Book 2", 
                Author = "Author 2",
                CategoryId = categoryId
            }
        };
        
        _mockBookRepository.Setup(repo => repo.GetByCategoryIdAsync(categoryId, pageNumber, pageSize))
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
        
        _mockBookRepository.Verify(repo => repo.GetByCategoryIdAsync(
            categoryId, pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNonExistentCategoryId_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetBooksByCategoryQuery(nonExistentCategoryId, pageNumber, pageSize);
        
        var emptyBooks = new List<LibraryManagementSystem.Domain.Entities.Book>();
        
        _mockBookRepository.Setup(repo => repo.GetByCategoryIdAsync(nonExistentCategoryId, pageNumber, pageSize))
            .ReturnsAsync(emptyBooks);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockBookRepository.Verify(repo => repo.GetByCategoryIdAsync(
            nonExistentCategoryId, pageNumber, pageSize), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetBooksByCategoryQuery(categoryId, pageNumber, pageSize);
        
        var expectedException = new Exception("Database error");
        
        _mockBookRepository.Setup(repo => repo.GetByCategoryIdAsync(categoryId, pageNumber, pageSize))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        
        _mockBookRepository.Verify(repo => repo.GetByCategoryIdAsync(
            categoryId, pageNumber, pageSize), Times.Once);
    }
} 