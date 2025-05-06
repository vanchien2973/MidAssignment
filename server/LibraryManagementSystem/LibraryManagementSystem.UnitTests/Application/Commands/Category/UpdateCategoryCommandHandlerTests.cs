using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Category;

public class UpdateCategoryCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<UpdateCategoryCommandHandler>> _mockLogger;
    private Mock<ICategoryRepository> _mockCategoryRepository;
    private UpdateCategoryCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateCategoryCommandHandler>>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();

        _mockUnitOfWork.Setup(uow => uow.Categories).Returns(_mockCategoryRepository.Object);

        _handler = new UpdateCategoryCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithExistingCategory_ReturnsTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand
        {
            CategoryId = categoryId,
            CategoryName = "Updated Fiction",
            Description = "Updated fiction books category"
        };

        var existingCategory = new LibraryManagementSystem.Domain.Entities.Category
        {
            CategoryId = categoryId,
            CategoryName = "Fiction",
            Description = "Fiction books category",
            CreatedDate = DateTime.UtcNow.AddDays(-1)
        };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(existingCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify category properties were updated
        Assert.That(existingCategory.CategoryName, Is.EqualTo(command.CategoryName));
        Assert.That(existingCategory.Description, Is.EqualTo(command.Description));
        
        // CreatedDate should not change
        Assert.That(existingCategory.CreatedDate, Is.LessThan(DateTime.UtcNow.AddMinutes(-1)));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockCategoryRepository.Verify(repo => repo.UpdateAsync(existingCategory), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentCategory_ReturnsFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand
        {
            CategoryId = categoryId,
            CategoryName = "Updated Fiction",
            Description = "Updated fiction books category"
        };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Category)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockCategoryRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Category>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand { CategoryId = categoryId };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
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
        Assert.Throws<ArgumentNullException>(() => new UpdateCategoryCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public void Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UpdateCategoryCommandHandler(_mockUnitOfWork.Object, null));
    }
}
