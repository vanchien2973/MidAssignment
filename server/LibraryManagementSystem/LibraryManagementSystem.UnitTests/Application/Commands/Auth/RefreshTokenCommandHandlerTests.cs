using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Auth;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Auth;

public class RefreshTokenCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ITokenService> _mockTokenService;
    private Mock<ILogger<RefreshTokenCommandHandler>> _mockLogger;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
    private RefreshTokenCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTokenService = new Mock<ITokenService>();
        _mockLogger = new Mock<ILogger<RefreshTokenCommandHandler>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);

        _handler = new RefreshTokenCommandHandler(
            _mockUnitOfWork.Object,
            _mockTokenService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidToken_ReturnsSuccessfulResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "expired-token", RefreshToken = "valid-refresh-token" };
        var userId = 1;
        var testUser = new LibraryManagementSystem.Domain.Entities.User { UserId = userId, Username = "testUser", IsActive = true };
        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";
        
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));

        _mockTokenService.Setup(service => service.GetPrincipalFromExpiredToken(command.Token))
            .Returns(principal);
        
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(testUser);
        
        _mockTokenService.Setup(service => service.GenerateAccessToken(testUser))
            .Returns(newAccessToken);
        
        _mockTokenService.Setup(service => service.GenerateRefreshToken())
            .Returns(newRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Token, Is.EqualTo(newAccessToken));
        Assert.That(result.RefreshToken, Is.EqualTo(newRefreshToken));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.UserActivityLogs.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithInvalidPrincipal_ReturnsFailureResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "invalid-token", RefreshToken = "refresh-token" };
        
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // No claims
        
        _mockTokenService.Setup(service => service.GetPrincipalFromExpiredToken(command.Token))
            .Returns(principal);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
    }

    [Test]
    public async Task Handle_WithNonExistentUser_ReturnsFailureResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "expired-token", RefreshToken = "refresh-token" };
        var userId = 999; // Non-existent user ID
        
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));

        _mockTokenService.Setup(service => service.GetPrincipalFromExpiredToken(command.Token))
            .Returns(principal);
        
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.User)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
    }

    [Test]
    public async Task Handle_WithInactiveUser_ReturnsFailureResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "expired-token", RefreshToken = "refresh-token" };
        var userId = 2;
        var inactiveUser = new LibraryManagementSystem.Domain.Entities.User { UserId = userId, Username = "inactiveUser", IsActive = false };
        
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));

        _mockTokenService.Setup(service => service.GetPrincipalFromExpiredToken(command.Token))
            .Returns(principal);
        
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(inactiveUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
    }

    [Test]
    public async Task Handle_WithException_ReturnsFailureResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "expired-token", RefreshToken = "refresh-token" };
        
        _mockTokenService.Setup(service => service.GetPrincipalFromExpiredToken(command.Token))
            .Throws(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
    }
}
