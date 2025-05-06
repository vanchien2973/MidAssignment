using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Book;

public class DeleteBookCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<DeleteBookCommandHandler>> _mockLogger;
    private Mock<IBookRepository> _mockBookRepository;
    private Mock<IBookBorrowingRequestDetailRepository> _mockBookBorrowingRequestDetailRepository;
    private DeleteBookCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<DeleteBookCommandHandler>>();
        _mockBookRepository = new Mock<IBookRepository>();
        _mockBookBorrowingRequestDetailRepository = new Mock<IBookBorrowingRequestDetailRepository>();

        _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequestDetails).Returns(_mockBookBorrowingRequestDetailRepository.Object);

        _handler = new DeleteBookCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithExistingBookAndNoBorrowings_ReturnsTrue()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new DeleteBookCommand { BookId = bookId };
        
        var existingBook = new LibraryManagementSystem.Domain.Entities.Book { BookId = bookId, Title = "Clean Code" };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(existingBook);

        _mockBookBorrowingRequestDetailRepository.Setup(repo => repo.HasActiveBorrowingsForBookAsync(bookId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockBookRepository.Verify(repo => repo.DeleteAsync(bookId), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentBook_ReturnsFalse()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new DeleteBookCommand { BookId = bookId };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Book)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockBookRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithActiveBorrowings_ReturnsFalse()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new DeleteBookCommand { BookId = bookId };
        
        var existingBook = new LibraryManagementSystem.Domain.Entities.Book { BookId = bookId, Title = "Clean Code" };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(existingBook);

        _mockBookBorrowingRequestDetailRepository.Setup(repo => repo.HasActiveBorrowingsForBookAsync(bookId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockBookRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new DeleteBookCommand { BookId = bookId };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
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
    public async Task Handle_WithNullUnitOfWork_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeleteBookCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public async Task Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeleteBookCommandHandler(_mockUnitOfWork.Object, null));
    }
}
