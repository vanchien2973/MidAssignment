using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Auth;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Auth
{
    [TestFixture]
    public class RegisterCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
        private Mock<ILogger<RegisterCommandHandler>> _mockLogger;
        private RegisterCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);
            
            _mockLogger = new Mock<ILogger<RegisterCommandHandler>>();
            
            _handler = new RegisterCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldRegisterUserAndReturnSuccess()
        {
            // Arrange
            var username = "newuser";
            var email = "new@example.com";
            var fullName = "New User";
            var password = "Password123";
            
            _mockUserRepository.Setup(repo => repo.UsernameExistsAsync(username))
                .ReturnsAsync(false);
            
            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(email))
                .ReturnsAsync(false);
            
            _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            
            _mockUserActivityLogRepository.Setup(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).Returns(Task.CompletedTask);
            
            var command = new RegisterCommand
            {
                Username = username,
                Email = email,
                FullName = fullName,
                Password = password
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Does.Contain("successful"));
            
            _mockUserRepository.Verify(repo => repo.UsernameExistsAsync(username), Times.Once);
            _mockUserRepository.Verify(repo => repo.EmailExistsAsync(email), Times.Once);
            _mockUserRepository.Verify(repo => repo.CreateAsync(It.Is<User>(u => 
                u.Username == username &&
                u.Email == email &&
                u.FullName == fullName)), Times.Once);
            _mockUserActivityLogRepository.Verify(repo => repo.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public void Handle_WithExistingUsername_ShouldThrowValidationException()
        {
            // Arrange
            var username = "existinguser";
            var email = "new@example.com";
            var fullName = "New User";
            var password = "Password123";
            
            _mockUserRepository.Setup(repo => repo.UsernameExistsAsync(username))
                .ReturnsAsync(true);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new RegisterCommand
            {
                Username = username,
                Email = email,
                FullName = fullName,
                Password = password
            };
            
            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => 
                await _handler.Handle(command, CancellationToken.None));
            
            Assert.That(ex.Errors, Is.Not.Empty);
            Assert.That(ex.Errors.First().PropertyName, Is.EqualTo("Username"));
            
            _mockUserRepository.Verify(repo => repo.UsernameExistsAsync(username), Times.Once);
            _mockUserRepository.Verify(repo => repo.CreateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public void Handle_WithExistingEmail_ShouldThrowValidationException()
        {
            // Arrange
            var username = "newuser";
            var email = "existing@example.com";
            var fullName = "New User";
            var password = "Password123";
            
            _mockUserRepository.Setup(repo => repo.UsernameExistsAsync(username))
                .ReturnsAsync(false);
            
            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(email))
                .ReturnsAsync(true);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new RegisterCommand
            {
                Username = username,
                Email = email,
                FullName = fullName,
                Password = password
            };
            
            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => 
                await _handler.Handle(command, CancellationToken.None));
            
            Assert.That(ex.Errors, Is.Not.Empty);
            Assert.That(ex.Errors.First().PropertyName, Is.EqualTo("Email"));
            
            _mockUserRepository.Verify(repo => repo.UsernameExistsAsync(username), Times.Once);
            _mockUserRepository.Verify(repo => repo.EmailExistsAsync(email), Times.Once);
            _mockUserRepository.Verify(repo => repo.CreateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WhenExceptionOccurs_ShouldReturnFailureResponse()
        {
            // Arrange
            var username = "newuser";
            var email = "new@example.com";
            var fullName = "New User";
            var password = "Password123";
            
            _mockUserRepository.Setup(repo => repo.UsernameExistsAsync(username))
                .ReturnsAsync(false);
            
            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(email))
                .ReturnsAsync(false);
            
            _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database error"));
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            
            var command = new RegisterCommand
            {
                Username = username,
                Email = email,
                FullName = fullName,
                Password = password
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("error"));
            
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }
    }
} 