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
    public class UpdateUserProfileCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IUserActivityLogRepository> _mockLogRepository;
        private Mock<ILogger<UpdateUserProfileCommandHandler>> _mockLogger;
        private Mock<ICurrentUserService> _mockCurrentUserService;
        private UpdateUserProfileCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogRepository = new Mock<IUserActivityLogRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockLogRepository.Object);
            
            _mockLogger = new Mock<ILogger<UpdateUserProfileCommandHandler>>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
            
            _handler = new UpdateUserProfileCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object, _mockCurrentUserService.Object);
        }

        [Test]
        public async Task Handle_WithValidData_ShouldUpdateUserProfile()
        {
            // Arrange
            var userId = 1;
            var newEmail = "newemail@example.com";
            var newFullName = "Updated Name";
            
            var user = new Domain.Entities.User
            {
                UserId = userId,
                UserName = "testuser",
                Email = "oldemail@example.com",
                FullName = "Old Name"
            };
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);
                
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(newEmail))
                .ReturnsAsync((Domain.Entities.User)null);
                
            _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.User>()))
                .Returns(Task.CompletedTask);
                
            _mockLogRepository.Setup(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()))
                .Returns(Task.CompletedTask);
                
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).Returns(Task.CompletedTask);
            
            var command = new UpdateUserProfileCommand
            {
                UserId = userId,
                Email = newEmail,
                FullName = newFullName
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.True);
            
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(newEmail), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.Is<Domain.Entities.User>(u => 
                u.UserId == userId && 
                u.Email == newEmail && 
                u.FullName == newFullName)), Times.Once);
            _mockLogRepository.Verify(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WithEmailInUse_ShouldReturnFalse()
        {
            // Arrange
            var userId = 1;
            var newEmail = "existing@example.com";
            var newFullName = "Updated Name";
            
            var user = new Domain.Entities.User
            {
                UserId = userId,
                UserName = "testuser",
                Email = "oldemail@example.com",
                FullName = "Old Name"
            };
            
            var existingUser = new Domain.Entities.User
            {
                UserId = 2, // Different user
                UserName = "otheruser",
                Email = newEmail,
                FullName = "Other User"
            };
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);
                
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(newEmail))
                .ReturnsAsync(existingUser);
                
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new UpdateUserProfileCommand
            {
                UserId = userId,
                Email = newEmail,
                FullName = newFullName
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.False);
            
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(newEmail), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Never);
            _mockLogRepository.Verify(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            var userId = 999;
            var newEmail = "newemail@example.com";
            var newFullName = "Updated Name";
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((Domain.Entities.User)null);
                
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new UpdateUserProfileCommand
            {
                UserId = userId,
                Email = newEmail,
                FullName = newFullName
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.False);
            
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Never);
            _mockLogRepository.Verify(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public void Handle_WhenExceptionOccurs_ShouldRethrowException()
        {
            // Arrange
            var userId = 1;
            var newEmail = "newemail@example.com";
            var newFullName = "Updated Name";
            
            var user = new Domain.Entities.User
            {
                UserId = userId,
                UserName = "testuser",
                Email = "oldemail@example.com",
                FullName = "Old Name"
            };
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);
                
            _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.User>()))
                .ThrowsAsync(new Exception("Database error"));
                
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new UpdateUserProfileCommand
            {
                UserId = userId,
                Email = newEmail,
                FullName = newFullName
            };
            
            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(command, CancellationToken.None));
            
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }
    }
} 