using FluentValidation;
using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Auth;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Auth;

public class RegisterCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<RegisterCommandHandler>> _mockLogger;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IUserActivityLogRepository> _mockUserActivityLogRepository;
    private RegisterCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<RegisterCommandHandler>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserActivityLogRepository = new Mock<IUserActivityLogRepository>();

        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.UserActivityLogs).Returns(_mockUserActivityLogRepository.Object);

        _handler = new RegisterCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidData_ReturnsSuccessfulResponse()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            Username = "newUser", 
            Password = "password123", 
            Email = "newuser@example.com", 
            FullName = "New User" 
        };
        
        int capturedUserId = 0;
        
        _mockUserRepository.Setup(repo => repo.UsernameExistsAsync(command.Username))
            .ReturnsAsync(false);
        
        _mockUserRepository.Setup(repo => repo.EmailExistsAsync(command.Email))
            .ReturnsAsync(false);
        
        _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()))
            .Callback<LibraryManagementSystem.Domain.Entities.User>(user => capturedUserId = 1)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("Registration successful"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockUserRepository.Verify(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.AtLeastOnce);
        _mockUnitOfWork.Verify(uow => uow.UserActivityLogs.LogActivityAsync(It.IsAny<UserActivityLog>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithExistingUsername_ThrowsValidationException()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            Username = "existingUser", 
            Password = "password123", 
            Email = "newuser@example.com", 
            FullName = "New User" 
        };
        
        _mockUserRepository.Setup(repo => repo.UsernameExistsAsync(command.Username))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () => 
            await _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Errors.Any(e => e.PropertyName == "Username"));
        Assert.That(ex.Errors.First(e => e.PropertyName == "Username").ErrorMessage, Is.EqualTo("Username already exists"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockUserRepository.Verify(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithExistingEmail_ThrowsValidationException()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            Username = "newUser", 
            Password = "password123", 
            Email = "existing@example.com", 
            FullName = "New User" 
        };
        
        _mockUserRepository.Setup(repo => repo.UsernameExistsAsync(command.Username))
            .ReturnsAsync(false);
        
        _mockUserRepository.Setup(repo => repo.EmailExistsAsync(command.Email))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () => 
            await _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Errors.Any(e => e.PropertyName == "Email"));
        Assert.That(ex.Errors.First(e => e.PropertyName == "Email").ErrorMessage, Is.EqualTo("Email already exists"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockUserRepository.Verify(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithException_ReturnsFailureResponse()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            Username = "newUser", 
            Password = "password123", 
            Email = "newuser@example.com", 
            FullName = "New User" 
        };
        
        _mockUserRepository.Setup(repo => repo.UsernameExistsAsync(command.Username))
            .ReturnsAsync(false);
        
        _mockUserRepository.Setup(repo => repo.EmailExistsAsync(command.Email))
            .ReturnsAsync(false);
        
        _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("An error occurred during registration"));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
    }

    [Test]
    public async Task Handle_CreatesUserWithCorrectProperties()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            Username = "newUser", 
            Password = "password123", 
            Email = "newuser@example.com", 
            FullName = "New User" 
        };
        
        LibraryManagementSystem.Domain.Entities.User capturedUser = null;
        
        _mockUserRepository.Setup(repo => repo.UsernameExistsAsync(command.Username))
            .ReturnsAsync(false);
        
        _mockUserRepository.Setup(repo => repo.EmailExistsAsync(command.Email))
            .ReturnsAsync(false);
        
        _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.User>()))
            .Callback<LibraryManagementSystem.Domain.Entities.User>(user => capturedUser = user)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(capturedUser, Is.Not.Null);
        Assert.That(capturedUser.Username, Is.EqualTo(command.Username));
        Assert.That(capturedUser.Email, Is.EqualTo(command.Email));
        Assert.That(capturedUser.FullName, Is.EqualTo(command.FullName));
        Assert.That(capturedUser.IsActive, Is.False);
        Assert.That(capturedUser.UserType, Is.EqualTo(UserType.NormalUser));
        
        // Check that password is hashed
        var hashedPassword = HashPassword(command.Password);
        Assert.That(capturedUser.Password, Is.EqualTo(hashedPassword));
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        return hash;
    }
}
