using LibraryManagementSystem.Application.Handlers.QueryHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.User;
using LibraryManagementSystem.Domain.Entities;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.User;

public class GetUserByIdQueryHandlerTests
{
    private Mock<IUserRepository> _mockUserRepository;
    private GetUserByIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new GetUserByIdQueryHandler(_mockUserRepository.Object);
    }

    [Test]
    public async Task Handle_WithExistingUserId_ReturnsUser()
    {
        // Arrange
        int userId = 1;
        var query = new GetUserByIdQuery(userId);
        
        var user = new LibraryManagementSystem.Domain.Entities.User 
        { 
            UserId = userId, 
            Username = "testUser",
            Email = "test@example.com",
            FullName = "Test User",
            IsActive = true
        };
        
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.Username, Is.EqualTo(user.Username));
        Assert.That(result.Email, Is.EqualTo(user.Email));
        Assert.That(result.FullName, Is.EqualTo(user.FullName));
        Assert.That(result.IsActive, Is.EqualTo(user.IsActive));
        
        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithNonExistentUserId_ReturnsNull()
    {
        // Arrange
        int nonExistentUserId = 999;
        var query = new GetUserByIdQuery(nonExistentUserId);
        
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(nonExistentUserId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.User)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Null);
        
        _mockUserRepository.Verify(repo => repo.GetByIdAsync(nonExistentUserId), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        int userId = 1;
        var query = new GetUserByIdQuery(userId);
        var expectedException = new Exception("Database connection error");
        
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () => 
            await _handler.Handle(query, CancellationToken.None));
        
        Assert.That(exception, Is.EqualTo(expectedException));
        
        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }
}
