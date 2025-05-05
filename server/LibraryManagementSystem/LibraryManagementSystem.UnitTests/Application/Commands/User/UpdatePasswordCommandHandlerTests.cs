using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.User
{
    [TestFixture]
    public class UpdatePasswordCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IUserActivityLogRepository> _mockActivityLogRepository;
        private Mock<IPasswordHashService> _mockPasswordHashService;
        private Mock<ILogger<UpdatePasswordCommandHandler>> _mockLogger;
        private UpdatePasswordCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockActivityLogRepository = new Mock<IUserActivityLogRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockActivityLogRepository.Object);
            
            _mockPasswordHashService = new Mock<IPasswordHashService>();
            _mockLogger = new Mock<ILogger<UpdatePasswordCommandHandler>>();
            
            _handler = new UpdatePasswordCommandHandler(
                _mockUnitOfWork.Object, 
                _mockLogger.Object, 
                _mockPasswordHashService.Object);
        }

        [Test]
        public async Task Handle_WithValidCredentials_ShouldUpdatePasswordAndReturnTrue()
        {
            // Arrange
            var userId = 1;
            var currentPassword = "currentPassword";
            var newPassword = "newPassword";
            var hashedCurrentPassword = "hashedCurrentPassword";
            var hashedNewPassword = "hashedNewPassword";
            
            var user = new Domain.Entities.User
            {
                UserId = userId,
                Username = "testuser",
                Password = hashedCurrentPassword,
                Email = "test@example.com"
            };

            var command = new UpdatePasswordCommand
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);
            
            _mockPasswordHashService.Setup(service => service.VerifyPassword(currentPassword, hashedCurrentPassword))
                .Returns(true);
            
            _mockPasswordHashService.Setup(service => service.HashPassword(newPassword))
                .Returns(hashedNewPassword);
            
            _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.User>()))
                .Returns(Task.CompletedTask);
            
            _mockActivityLogRepository.Setup(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mockPasswordHashService.Verify(service => service.VerifyPassword(currentPassword, hashedCurrentPassword), Times.Once);
            _mockPasswordHashService.Verify(service => service.HashPassword(newPassword), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.Is<Domain.Entities.User>(u => u.Password == hashedNewPassword)), Times.Once);
            _mockActivityLogRepository.Verify(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WhenUserNotFound_ShouldReturnFalse()
        {
            // Arrange
            var userId = 1;
            var currentPassword = "currentPassword";
            var newPassword = "newPassword";
            
            var command = new UpdatePasswordCommand
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((Domain.Entities.User)null);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mockPasswordHashService.Verify(service => service.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockPasswordHashService.Verify(service => service.HashPassword(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Never);
            _mockActivityLogRepository.Verify(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WithIncorrectCurrentPassword_ShouldReturnFalse()
        {
            // Arrange
            var userId = 1;
            var currentPassword = "wrongPassword";
            var newPassword = "newPassword";
            var hashedCurrentPassword = "hashedCurrentPassword";
            
            var user = new Domain.Entities.User
            {
                UserId = userId,
                Username = "testuser",
                Password = hashedCurrentPassword,
                Email = "test@example.com"
            };

            var command = new UpdatePasswordCommand
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);
            
            _mockPasswordHashService.Setup(service => service.VerifyPassword(currentPassword, hashedCurrentPassword))
                .Returns(false);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mockPasswordHashService.Verify(service => service.VerifyPassword(currentPassword, hashedCurrentPassword), Times.Once);
            _mockPasswordHashService.Verify(service => service.HashPassword(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Never);
            _mockActivityLogRepository.Verify(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public void Handle_WhenExceptionOccurs_ShouldRollbackAndThrowException()
        {
            // Arrange
            var userId = 1;
            var currentPassword = "currentPassword";
            var newPassword = "newPassword";
            var hashedCurrentPassword = "hashedCurrentPassword";
            var hashedNewPassword = "hashedNewPassword";
            
            var user = new Domain.Entities.User
            {
                UserId = userId,
                Username = "testuser",
                Password = hashedCurrentPassword,
                Email = "test@example.com"
            };

            var command = new UpdatePasswordCommand
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);
            
            _mockPasswordHashService.Setup(service => service.VerifyPassword(currentPassword, hashedCurrentPassword))
                .Returns(true);
            
            _mockPasswordHashService.Setup(service => service.HashPassword(newPassword))
                .Returns(hashedNewPassword);
            
            _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.User>()))
                .ThrowsAsync(new Exception("Database error"));
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(command, CancellationToken.None));
            
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mockPasswordHashService.Verify(service => service.VerifyPassword(currentPassword, hashedCurrentPassword), Times.Once);
            _mockPasswordHashService.Verify(service => service.HashPassword(newPassword), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }
    }
} 