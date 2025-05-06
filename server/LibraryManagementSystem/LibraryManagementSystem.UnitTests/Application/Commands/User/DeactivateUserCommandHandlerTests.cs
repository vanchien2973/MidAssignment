using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.User;

public class DeactivateUserCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<DeactivateUserCommandHandler>> _mockLogger;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
    private DeactivateUserCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<DeactivateUserCommandHandler>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);

        _handler = new DeactivateUserCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithActiveUser_DeactivatesUserAndReturnsTrue()
    {
        // Arrange
        int userId = 1;
        var command = new DeactivateUserCommand(userId);
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            IsActive = true
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
        Assert.That(user.IsActive, Is.False);
        
        Assert.That(capturedLog, Is.Not.Null);
        Assert.That(capturedLog.UserId, Is.EqualTo(userId));
        Assert.That(capturedLog.ActivityType, Is.EqualTo("UserDeactivated"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task Handle_WithAlreadyInactiveUser_ReturnsTrue()
    {
        // Arrange
        int userId = 1;
        var command = new DeactivateUserCommand(userId);
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            IsActive = false // Already inactive
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        int userId = 1;
        var command = new DeactivateUserCommand(userId);

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
    }

    [Test]
    public void Handle_WithNullUnitOfWork_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeactivateUserCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public void Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeactivateUserCommandHandler(_mockUnitOfWork.Object, null));
    }
}
