using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.User;

public class UpdateUserRoleCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<UpdateUserRoleCommandHandler>> _mockLogger;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
    private UpdateUserRoleCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateUserRoleCommandHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);
        
        _mockCurrentUserService.Setup(service => service.UserId).Returns(2); // Admin user ID (different from target user)

        _handler = new UpdateUserRoleCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object
        );
    }

    [Test]
    public async Task Handle_WithValidRole_UpdatesRoleAndReturnsTrue()
    {
        // Arrange
        int userId = 1;
        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            UserType = UserType.SuperUser
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            UserType = UserType.NormalUser // Starting role
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        UserActivityLog capturedLog = null;
        _mockUserActivityLogRepository.Setup(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()))
            .Callback<UserActivityLog>(log => capturedLog = log)
            .ReturnsAsync((UserActivityLog)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(user.UserType, Is.EqualTo(UserType.SuperUser));
        
        Assert.That(capturedLog, Is.Not.Null);
        Assert.That(capturedLog.UserId, Is.EqualTo(2)); // Admin user ID
        Assert.That(capturedLog.ActivityType, Is.EqualTo("UserRoleUpdated"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task Handle_WithOwnUserId_ThrowsInvalidOperationException()
    {
        // Arrange
        int adminUserId = 2; // Same as current user ID in mock setup
        var command = new UpdateUserRoleCommand
        {
            UserId = adminUserId, // Trying to update own role
            UserType = UserType.NormalUser
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = adminUserId,
            Username = "adminuser",
            UserType = UserType.SuperUser
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(adminUserId))
            .ReturnsAsync(user);
            
        _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.That(ex.Message, Is.EqualTo("You cannot change your own role"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        int userId = 1;
        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            UserType = UserType.SuperUser
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.User)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()), Times.Never);
    }

    [Test]
    public void Handle_WhenExceptionOccurs_RollsBackTransactionAndThrowsException()
    {
        // Arrange
        int userId = 1;
        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            UserType = UserType.SuperUser
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            UserType = UserType.NormalUser
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
            
        _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.That(ex.Message, Is.EqualTo("Test exception"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
    }
}
