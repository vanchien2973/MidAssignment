using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.User;

public class UpdatePasswordCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<UpdatePasswordCommandHandler>> _mockLogger;
    private Mock<IPasswordHashService> _mockPasswordHashService;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
    private UpdatePasswordCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdatePasswordCommandHandler>>();
        _mockPasswordHashService = new Mock<IPasswordHashService>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);

        _handler = new UpdatePasswordCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockPasswordHashService.Object
        );
    }

    [Test]
    public async Task Handle_WithValidPasswordChange_ReturnsTrue()
    {
        // Arrange
        int userId = 1;
        string currentPassword = "currentPassword";
        string currentPasswordHash = "hashedCurrentPassword";
        string newPassword = "newPassword";
        string newPasswordHash = "hashedNewPassword";
        
        var command = new UpdatePasswordCommand
        {
            UserId = userId,
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            Password = currentPasswordHash
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
            
        _mockPasswordHashService.Setup(service => service.VerifyPassword(currentPassword, currentPasswordHash))
            .Returns(true);
            
        _mockPasswordHashService.Setup(service => service.HashPassword(newPassword))
            .Returns(newPasswordHash);

        UserActivityLog capturedLog = null;
        _mockUserActivityLogRepository.Setup(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()))
            .Callback<UserActivityLog>(log => capturedLog = log)
            .ReturnsAsync((UserActivityLog)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(user.Password, Is.EqualTo(newPasswordHash));
        
        Assert.That(capturedLog, Is.Not.Null);
        Assert.That(capturedLog.UserId, Is.EqualTo(userId));
        Assert.That(capturedLog.ActivityType, Is.EqualTo("PasswordChanged"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task Handle_WithInvalidCurrentPassword_ReturnsFalse()
    {
        // Arrange
        int userId = 1;
        string currentPassword = "incorrectPassword";
        string currentPasswordHash = "hashedCurrentPassword";
        string newPassword = "newPassword";
        
        var command = new UpdatePasswordCommand
        {
            UserId = userId,
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            Password = currentPasswordHash
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
            
        _mockPasswordHashService.Setup(service => service.VerifyPassword(currentPassword, currentPasswordHash))
            .Returns(false); // Password verification fails

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(user.Password, Is.EqualTo(currentPasswordHash)); // Password remains unchanged
        
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
        var command = new UpdatePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "currentPassword",
            NewPassword = "newPassword"
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
        _mockPasswordHashService.Verify(service => service.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Handle_WhenExceptionOccurs_RollsBackTransactionAndThrowsException()
    {
        // Arrange
        int userId = 1;
        string currentPassword = "currentPassword";
        string currentPasswordHash = "hashedCurrentPassword";
        string newPassword = "newPassword";
        
        var command = new UpdatePasswordCommand
        {
            UserId = userId,
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            Password = currentPasswordHash
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
            
        _mockPasswordHashService.Setup(service => service.VerifyPassword(currentPassword, currentPasswordHash))
            .Returns(true);
            
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
