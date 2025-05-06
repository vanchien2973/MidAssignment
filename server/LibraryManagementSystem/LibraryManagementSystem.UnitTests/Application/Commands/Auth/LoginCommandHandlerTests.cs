using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Auth;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Auth;

public class LoginCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ITokenService> _mockTokenService;
    private Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
    private LoginCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTokenService = new Mock<ITokenService>();
        _mockLogger = new Mock<ILogger<LoginCommandHandler>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);

        _handler = new LoginCommandHandler(
            _mockUnitOfWork.Object,
            _mockTokenService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidCredentials_ReturnsSuccessfulResponse()
    {
        // Arrange
        var command = new LoginCommand { Username = "testUser", Password = "password123" };
        var testUser = new LibraryManagementSystem.Domain.Entities.User 
        { 
            UserId = 1, 
            Username = "testUser", 
            Password = HashPassword("password123"), 
            IsActive = true 
        };
        var accessToken = "test-access-token";
        var refreshToken = "test-refresh-token";

        _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(command.Username))
            .ReturnsAsync(testUser);
        
        _mockTokenService.Setup(service => service.GenerateAccessToken(testUser))
            .Returns(accessToken);
        
        _mockTokenService.Setup(service => service.GenerateRefreshToken())
            .Returns(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Token, Is.EqualTo(accessToken));
        Assert.That(result.RefreshToken, Is.EqualTo(refreshToken));
        Assert.That(result.User, Is.SameAs(testUser));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(testUser), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Test]
    public async Task Handle_WithNonExistentUser_ReturnsFailureResponse()
    {
        // Arrange
        var command = new LoginCommand { Username = "nonExistentUser", Password = "password123" };
        
        _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(command.Username))
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
        var command = new LoginCommand { Username = "inactiveUser", Password = "password123" };
        var inactiveUser = new LibraryManagementSystem.Domain.Entities.User 
        { 
            UserId = 2, 
            Username = "inactiveUser", 
            Password = HashPassword("password123"), 
            IsActive = false 
        };
        
        _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(command.Username))
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
    public async Task Handle_WithInvalidPassword_ReturnsFailureResponse()
    {
        // Arrange
        var command = new LoginCommand { Username = "testUser", Password = "wrongPassword" };
        var testUser = new LibraryManagementSystem.Domain.Entities.User 
        { 
            UserId = 1, 
            Username = "testUser", 
            Password = HashPassword("password123"), 
            IsActive = true 
        };
        
        _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(command.Username))
            .ReturnsAsync(testUser);

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
        var command = new LoginCommand { Username = "testUser", Password = "password123" };
        
        _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(command.Username))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        return hash;
    }
}
