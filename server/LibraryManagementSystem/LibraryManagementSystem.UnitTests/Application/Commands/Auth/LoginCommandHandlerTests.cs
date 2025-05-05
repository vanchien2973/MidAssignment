using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Auth;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Auth
{
    [TestFixture]
    public class LoginCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
        private Mock<ITokenService> _mockTokenService;
        private Mock<ILogger<LoginCommandHandler>> _mockLogger;
        private LoginCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);
            
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<LoginCommandHandler>>();
            
            _handler = new LoginCommandHandler(_mockUnitOfWork.Object, _mockTokenService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WithValidCredentials_ShouldReturnSuccessResponse()
        {
            // Arrange
            var username = "testuser";
            var password = "Password123"; // Plain text password
            var hashedPassword = HashPassword(password); // The same hash algorithm used in LoginCommandHandler
            
            var user = new User
            {
                UserId = 1,
                UserName = username,
                Password = hashedPassword,
                Email = "test@example.com",
                FullName = "Test User",
                Role = UserRole.Member,
                IsActive = true
            };
            
            var accessToken = "test_access_token";
            var refreshToken = "test_refresh_token";
            
            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(username))
                .ReturnsAsync(user);
            
            _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            
            _mockUserActivityLogRepository.Setup(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).Returns(Task.CompletedTask);
            
            _mockTokenService.Setup(service => service.GenerateAccessToken(user))
                .Returns(accessToken);
            
            _mockTokenService.Setup(service => service.GenerateRefreshToken())
                .Returns(refreshToken);
            
            var command = new LoginCommand
            {
                Username = username,
                Password = password
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Token, Is.EqualTo(accessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(refreshToken));
            Assert.That(result.User, Is.EqualTo(user));
            
            _mockUserRepository.Verify(repo => repo.GetByUsernameAsync(username), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.Is<User>(u => 
                u.UserId == user.UserId && 
                u.LastLoginDate != null)), Times.Once);
            _mockUserActivityLogRepository.Verify(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WithInvalidUsername_ShouldReturnFailureResponse()
        {
            // Arrange
            var username = "nonexistentuser";
            var password = "Password123";
            
            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(username))
                .ReturnsAsync((User)null);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new LoginCommand
            {
                Username = username,
                Password = password
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Token, Is.Null);
            Assert.That(result.RefreshToken, Is.Null);
            Assert.That(result.User, Is.Null);
            
            _mockUserRepository.Verify(repo => repo.GetByUsernameAsync(username), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUserActivityLogRepository.Verify(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WithInactiveUser_ShouldReturnFailureResponse()
        {
            // Arrange
            var username = "inactiveuser";
            var password = "Password123";
            var hashedPassword = HashPassword(password);
            
            var user = new User
            {
                UserId = 1,
                UserName = username,
                Password = hashedPassword,
                Email = "test@example.com",
                FullName = "Test User",
                Role = UserRole.Member,
                IsActive = false // Inactive user
            };
            
            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(username))
                .ReturnsAsync(user);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new LoginCommand
            {
                Username = username,
                Password = password
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.False);
            
            _mockUserRepository.Verify(repo => repo.GetByUsernameAsync(username), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WithIncorrectPassword_ShouldReturnFailureResponse()
        {
            // Arrange
            var username = "testuser";
            var correctPassword = "Password123";
            var incorrectPassword = "WrongPassword";
            var hashedPassword = HashPassword(correctPassword);
            
            var user = new User
            {
                UserId = 1,
                UserName = username,
                Password = hashedPassword,
                Email = "test@example.com",
                FullName = "Test User",
                Role = UserRole.Member,
                IsActive = true
            };
            
            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(username))
                .ReturnsAsync(user);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new LoginCommand
            {
                Username = username,
                Password = incorrectPassword
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.False);
            
            _mockUserRepository.Verify(repo => repo.GetByUsernameAsync(username), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WhenExceptionOccurs_ShouldReturnFailureResponse()
        {
            // Arrange
            var username = "testuser";
            var password = "Password123";
            
            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(username))
                .ThrowsAsync(new Exception("Database error"));
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new LoginCommand
            {
                Username = username,
                Password = password
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.False);
            
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        // Helper method to simulate the same hashing algorithm used in LoginCommandHandler
        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hash;
        }
    }
} 