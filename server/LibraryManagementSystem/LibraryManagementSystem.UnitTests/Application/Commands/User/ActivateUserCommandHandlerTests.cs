using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.User;

public class ActivateUserCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<ActivateUserCommandHandler>> _mockLogger;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
    private ActivateUserCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ActivateUserCommandHandler>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);

        _handler = new ActivateUserCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithInactiveUser_ActivatesUserAndReturnsTrue()
    {
        // Arrange
        int userId = 1;
        var command = new ActivateUserCommand(userId);
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            IsActive = false
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
        Assert.That(user.IsActive, Is.True);
        
        Assert.That(capturedLog, Is.Not.Null);
        Assert.That(capturedLog.UserId, Is.EqualTo(userId));
        Assert.That(capturedLog.ActivityType, Is.EqualTo("UserActivated"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task Handle_WithAlreadyActiveUser_ReturnsTrue()
    {
        // Arrange
        int userId = 1;
        var command = new ActivateUserCommand(userId);
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            IsActive = true // Already active
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
        var command = new ActivateUserCommand(userId);

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
        Assert.Throws<ArgumentNullException>(() => new ActivateUserCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public void Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ActivateUserCommandHandler(_mockUnitOfWork.Object, null));
    }
}
