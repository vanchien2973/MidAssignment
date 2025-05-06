using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.User;

public class UpdateUserProfileCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<UpdateUserProfileCommandHandler>> _mockLogger;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
    private UpdateUserProfileCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateUserProfileCommandHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);
        
        _mockCurrentUserService.Setup(service => service.UserId).Returns(1); // Same user updating their profile

        _handler = new UpdateUserProfileCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object
        );
    }

    [Test]
    public async Task Handle_WithValidProfileUpdate_ReturnsTrue()
    {
        // Arrange
        int userId = 1;
        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            Email = "newemail@example.com",
            FullName = "New Full Name"
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            Email = "oldemail@example.com",
            FullName = "Old Full Name"
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
            
        // No existing user with the same email
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.User)null);

        UserActivityLog capturedLog = null;
        _mockUserActivityLogRepository.Setup(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()))
            .Callback<UserActivityLog>(log => capturedLog = log)
            .ReturnsAsync((UserActivityLog)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(user.Email, Is.EqualTo(command.Email));
        Assert.That(user.FullName, Is.EqualTo(command.FullName));
        
        Assert.That(capturedLog, Is.Not.Null);
        Assert.That(capturedLog.UserId, Is.EqualTo(userId));
        Assert.That(capturedLog.ActivityType, Is.EqualTo("ProfileUpdated"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task Handle_WithEmailAlreadyInUse_ReturnsFalse()
    {
        // Arrange
        int userId = 1;
        string newEmail = "existing@example.com";
        
        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            Email = newEmail,
            FullName = "New Full Name"
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            Email = "oldemail@example.com",
            FullName = "Old Full Name"
        };
        
        var existingUser = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = 2, // Different user
            Email = newEmail
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
            
        // Email already in use by another user
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(newEmail))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(user.Email, Is.EqualTo("oldemail@example.com")); // Email not changed
        Assert.That(user.FullName, Is.EqualTo("Old Full Name")); // Full name not changed
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithEmailAlreadyInUseBySameUser_UpdatesProfile()
    {
        // Arrange
        int userId = 1;
        string email = "existing@example.com";
        
        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            Email = email, // Same email
            FullName = "New Full Name"
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            Email = email, // Already has this email
            FullName = "Old Full Name"
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(user.Email, Is.EqualTo(email)); // Email remains the same
        Assert.That(user.FullName, Is.EqualTo("New Full Name")); // Full name updated
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        int userId = 1;
        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            Email = "newemail@example.com",
            FullName = "New Full Name"
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
        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            Email = "newemail@example.com",
            FullName = "New Full Name"
        };
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser",
            Email = "oldemail@example.com",
            FullName = "Old Full Name"
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
            
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.User)null);
            
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
