using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.User;

public class DeleteUserCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<ILogger<DeleteUserCommandHandler>> _mockLogger;
    private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
    private DeleteUserCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<DeleteUserCommandHandler>>();
        _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);
        
        _mockCurrentUserService.Setup(service => service.UserId).Returns(999); // Admin user ID

        _handler = new DeleteUserCommandHandler(
            _mockUserRepository.Object,
            _mockUnitOfWork.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidUser_DeletesUserAndReturnsTrue()
    {
        // Arrange
        int userId = 1;
        var command = new DeleteUserCommand(userId);
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser"
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
        
        Assert.That(capturedLog, Is.Not.Null);
        Assert.That(capturedLog.UserId, Is.EqualTo(999)); // Admin user ID
        Assert.That(capturedLog.ActivityType, Is.EqualTo("UserDeleted"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUserRepository.Verify(repo => repo.DeleteAsync(userId), Times.Once);
    }

    [Test]
    public void Handle_WhenExceptionOccurs_RollsBackTransactionAndThrowsException()
    {
        // Arrange
        int userId = 1;
        var command = new DeleteUserCommand(userId);
        
        var user = new LibraryManagementSystem.Domain.Entities.User
        {
            UserId = userId,
            Username = "testuser"
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
            
        _mockUserRepository.Setup(repo => repo.DeleteAsync(userId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.That(ex.Message, Is.EqualTo("Test exception"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
    }
}
