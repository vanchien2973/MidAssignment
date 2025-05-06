using LibraryManagementSystem.Application.Handlers.QueryHandlers.Auth;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Auth;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Auth;

public class GetCurrentUserQueryHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<GetCurrentUserQueryHandler>> _mockLogger;
    private Mock<IUserRepository> _mockUserRepository;
    private GetCurrentUserQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<GetCurrentUserQueryHandler>>();
        _mockUserRepository = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);

        _handler = new GetCurrentUserQueryHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidUserId_ReturnsUserResponse()
    {
        // Arrange
        string userIdString = "1";
        int userId = 1;
        var query = new GetCurrentUserQuery { UserId = userIdString };
        var user = new LibraryManagementSystem.Domain.Entities.User 
        { 
            UserId = userId, 
            Username = "testUser",
            Email = "test@example.com",
            IsActive = true 
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Username, Is.EqualTo(user.Username));
        Assert.That(result.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public async Task Handle_WithInvalidUserId_ReturnsFailureResponse()
    {
        // Arrange
        string invalidUserId = "invalid";
        var query = new GetCurrentUserQuery { UserId = invalidUserId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        
        _mockUserRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithNonExistentUserId_ReturnsFailureResponse()
    {
        // Arrange
        string userIdString = "999";
        int userId = 999;
        var query = new GetCurrentUserQuery { UserId = userIdString };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.User)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        
        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFailureResponse()
    {
        // Arrange
        string userIdString = "1";
        int userId = 1;
        var query = new GetCurrentUserQuery { UserId = userIdString };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        
        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
} 