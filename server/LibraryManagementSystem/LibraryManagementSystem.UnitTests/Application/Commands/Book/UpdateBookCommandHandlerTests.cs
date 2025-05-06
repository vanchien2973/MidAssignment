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

public class UpdateBookCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<UpdateBookCommandHandler>> _mockLogger;
    private Mock<IBookRepository> _mockBookRepository;
    private UpdateBookCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateBookCommandHandler>>();
        _mockBookRepository = new Mock<IBookRepository>();

        _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);

        _handler = new UpdateBookCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithExistingBook_ReturnsTrue()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand
        {
            BookId = bookId,
            Title = "Updated Clean Code",
            Author = "Robert C. Martin",
            CategoryId = Guid.NewGuid(),
            ISBN = "978-0132350884",
            PublishedYear = 2009,
            Publisher = "Prentice Hall",
            Description = "Updated description",
            TotalCopies = 15
        };

        var existingBook = new LibraryManagementSystem.Domain.Entities.Book
        {
            BookId = bookId,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            CategoryId = Guid.NewGuid(),
            ISBN = "978-0132350884",
            PublishedYear = 2008,
            Publisher = "Prentice Hall",
            Description = "A book about writing clean code",
            TotalCopies = 10,
            AvailableCopies = 5
        };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(existingBook);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify book properties were updated
        Assert.That(existingBook.Title, Is.EqualTo(command.Title));
        Assert.That(existingBook.Author, Is.EqualTo(command.Author));
        Assert.That(existingBook.CategoryId, Is.EqualTo(command.CategoryId));
        Assert.That(existingBook.ISBN, Is.EqualTo(command.ISBN));
        Assert.That(existingBook.PublishedYear, Is.EqualTo(command.PublishedYear));
        Assert.That(existingBook.Publisher, Is.EqualTo(command.Publisher));
        Assert.That(existingBook.Description, Is.EqualTo(command.Description));
        Assert.That(existingBook.TotalCopies, Is.EqualTo(command.TotalCopies));
        
        // Verify available copies was adjusted correctly (original 5 + difference of 5 = 10)
        Assert.That(existingBook.AvailableCopies, Is.EqualTo(10));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(existingBook), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentBook_ReturnsFalse()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand
        {
            BookId = bookId,
            Title = "Updated Clean Code",
            Author = "Robert C. Martin"
        };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Book)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenDecreasingTotalCopies_AdjustsAvailableCopiesCorrectly()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand
        {
            BookId = bookId,
            Title = "Updated Clean Code",
            Author = "Robert C. Martin",
            TotalCopies = 8 // Decreasing from 10 to 8
        };

        var existingBook = new LibraryManagementSystem.Domain.Entities.Book
        {
            BookId = bookId,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            TotalCopies = 10,
            AvailableCopies = 5
        };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(existingBook);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify available copies was adjusted correctly (original 5 + difference of -2 = 3)
        Assert.That(existingBook.TotalCopies, Is.EqualTo(8));
        Assert.That(existingBook.AvailableCopies, Is.EqualTo(3));
    }

    [Test]
    public async Task Handle_WhenAvailableCopiesWouldBecomeNegative_SetsAvailableCopiesTo0()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand
        {
            BookId = bookId,
            Title = "Updated Clean Code",
            Author = "Robert C. Martin",
            TotalCopies = 3 // Decreasing from 10 to 3
        };

        var existingBook = new LibraryManagementSystem.Domain.Entities.Book
        {
            BookId = bookId,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            TotalCopies = 10,
            AvailableCopies = 5
        };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(existingBook);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify available copies was adjusted to 0 instead of becoming negative
        Assert.That(existingBook.TotalCopies, Is.EqualTo(3));
        Assert.That(existingBook.AvailableCopies, Is.EqualTo(0));
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand { BookId = bookId };

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
        Assert.Throws<ArgumentNullException>(() => new UpdateBookCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public async Task Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UpdateBookCommandHandler(_mockUnitOfWork.Object, null));
    }
}
