using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Category;

public class CreateCategoryCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<CreateCategoryCommandHandler>> _mockLogger;
    private Mock<ICategoryRepository> _mockCategoryRepository;
    private CreateCategoryCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<CreateCategoryCommandHandler>>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();

        _mockUnitOfWork.Setup(uow => uow.Categories).Returns(_mockCategoryRepository.Object);

        _handler = new CreateCategoryCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidCommand_ReturnsTrue()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            CategoryName = "Fiction",
            Description = "Fiction books category"
        };

        LibraryManagementSystem.Domain.Entities.Category capturedCategory = null;

        _mockCategoryRepository.Setup(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Category>()))
            .Callback<LibraryManagementSystem.Domain.Entities.Category>(category => capturedCategory = category)
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Category category) => category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        Assert.That(capturedCategory, Is.Not.Null);
        Assert.That(capturedCategory.CategoryId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(capturedCategory.CategoryName, Is.EqualTo(command.CategoryName));
        Assert.That(capturedCategory.Description, Is.EqualTo(command.Description));
        Assert.That(capturedCategory.CreatedDate, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockCategoryRepository.Verify(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Category>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            CategoryName = "Fiction",
            Description = "Fiction books category"
        };

        _mockCategoryRepository.Setup(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Category>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
    }

    [Test]
    public void Handle_WithNullUnitOfWork_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateCategoryCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public void Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateCategoryCommandHandler(_mockUnitOfWork.Object, null));
    }
}
