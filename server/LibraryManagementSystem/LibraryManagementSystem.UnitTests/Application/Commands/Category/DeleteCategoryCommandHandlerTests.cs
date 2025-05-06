using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Category;

public class DeleteCategoryCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<DeleteCategoryCommandHandler>> _mockLogger;
    private Mock<ICategoryRepository> _mockCategoryRepository;
    private Mock<IBookRepository> _mockBookRepository;
    private DeleteCategoryCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<DeleteCategoryCommandHandler>>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockBookRepository = new Mock<IBookRepository>();

        _mockUnitOfWork.Setup(uow => uow.Categories).Returns(_mockCategoryRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);

        _handler = new DeleteCategoryCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithExistingCategoryAndNoBooks_ReturnsTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand { CategoryId = categoryId };
        
        var existingCategory = new LibraryManagementSystem.Domain.Entities.Category 
        { 
            CategoryId = categoryId, 
            CategoryName = "Fiction" 
        };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(existingCategory);
            
        _mockBookRepository.Setup(repo => repo.HasBooksInCategoryAsync(categoryId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockCategoryRepository.Verify(repo => repo.DeleteAsync(categoryId), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentCategory_ReturnsFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand { CategoryId = categoryId };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Category)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockCategoryRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithCategoryHavingBooks_ReturnsFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand { CategoryId = categoryId };
        
        var existingCategory = new LibraryManagementSystem.Domain.Entities.Category 
        { 
            CategoryId = categoryId, 
            CategoryName = "Fiction" 
        };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(existingCategory);
            
        _mockBookRepository.Setup(repo => repo.HasBooksInCategoryAsync(categoryId))
            .ReturnsAsync(true); // Category has books

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockCategoryRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand { CategoryId = categoryId };

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
        Assert.Throws<ArgumentNullException>(() => new DeleteCategoryCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public void Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeleteCategoryCommandHandler(_mockUnitOfWork.Object, null));
    }
}
