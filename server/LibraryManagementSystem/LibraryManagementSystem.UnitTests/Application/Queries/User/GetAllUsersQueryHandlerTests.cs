using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.User;
using LibraryManagementSystem.Domain.Entities;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.User;

public class GetAllUsersQueryHandlerTests
{
    private Mock<IUserRepository> _mockUserRepository;
    private GetAllUsersQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new GetAllUsersQueryHandler(_mockUserRepository.Object);
    }

    [Test]
    public async Task Handle_ReturnsCorrectPaginatedUsers()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string searchTerm = "test";
        int totalCount = 15;
        
        var query = new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };
        
        var users = new List<LibraryManagementSystem.Domain.Entities.User>
        {
            new() { UserId = 1, Username = "testUser1", Email = "test1@example.com" },
            new() { UserId = 2, Username = "testUser2", Email = "test2@example.com" }
        };
        
        _mockUserRepository.Setup(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm))
            .ReturnsAsync(users);
        
        _mockUserRepository.Setup(repo => repo.CountBySearchTermAsync(searchTerm))
            .ReturnsAsync(totalCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count(), Is.EqualTo(users.Count));
        Assert.That(result.TotalCount, Is.EqualTo(totalCount));
        Assert.That(result.PageNumber, Is.EqualTo(pageNumber));
        Assert.That(result.PageSize, Is.EqualTo(pageSize));
        
        _mockUserRepository.Verify(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm), Times.Once);
        _mockUserRepository.Verify(repo => repo.CountBySearchTermAsync(searchTerm), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithEmptySearchTerm_ReturnsAllUsers()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string searchTerm = "";
        int totalCount = 3;
        
        var query = new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };
        
        var users = new List<LibraryManagementSystem.Domain.Entities.User>
        {
            new() { UserId = 1, Username = "user1", Email = "user1@example.com" },
            new() { UserId = 2, Username = "user2", Email = "user2@example.com" },
            new() { UserId = 3, Username = "user3", Email = "user3@example.com" }
        };
        
        _mockUserRepository.Setup(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm))
            .ReturnsAsync(users);
        
        _mockUserRepository.Setup(repo => repo.CountBySearchTermAsync(searchTerm))
            .ReturnsAsync(totalCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count(), Is.EqualTo(users.Count));
        Assert.That(result.TotalCount, Is.EqualTo(totalCount));
        
        _mockUserRepository.Verify(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm), Times.Once);
        _mockUserRepository.Verify(repo => repo.CountBySearchTermAsync(searchTerm), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string searchTerm = "nonexistent";
        int totalCount = 0;
        
        var query = new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };
        
        var emptyUserList = new List<LibraryManagementSystem.Domain.Entities.User>();
        
        _mockUserRepository.Setup(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm))
            .ReturnsAsync(emptyUserList);
        
        _mockUserRepository.Setup(repo => repo.CountBySearchTermAsync(searchTerm))
            .ReturnsAsync(totalCount);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count(), Is.EqualTo(0));
        Assert.That(result.TotalCount, Is.EqualTo(totalCount));
        
        _mockUserRepository.Verify(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm), Times.Once);
        _mockUserRepository.Verify(repo => repo.CountBySearchTermAsync(searchTerm), Times.Once);
    }
}
