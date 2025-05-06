using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Book;

public class CreateBookCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<CreateBookCommandHandler>> _mockLogger;
    private Mock<IBookRepository> _mockBookRepository;
    private CreateBookCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<CreateBookCommandHandler>>();
        _mockBookRepository = new Mock<IBookRepository>();

        _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);

        _handler = new CreateBookCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidData_ReturnsTrue()
    {
        // Arrange
        var command = new CreateBookCommand
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            CategoryId = Guid.NewGuid(),
            ISBN = "978-0132350884",
            PublishedYear = 2008,
            Publisher = "Prentice Hall",
            Description = "A book about writing clean code",
            TotalCopies = 10
        };

        LibraryManagementSystem.Domain.Entities.Book capturedBook = null;

        _mockBookRepository.Setup(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()))
            .Callback<LibraryManagementSystem.Domain.Entities.Book>(book => capturedBook = book)
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Book book) => book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        Assert.That(capturedBook, Is.Not.Null);
        Assert.That(capturedBook.Title, Is.EqualTo(command.Title));
        Assert.That(capturedBook.Author, Is.EqualTo(command.Author));
        Assert.That(capturedBook.CategoryId, Is.EqualTo(command.CategoryId));
        Assert.That(capturedBook.ISBN, Is.EqualTo(command.ISBN));
        Assert.That(capturedBook.PublishedYear, Is.EqualTo(command.PublishedYear));
        Assert.That(capturedBook.Publisher, Is.EqualTo(command.Publisher));
        Assert.That(capturedBook.Description, Is.EqualTo(command.Description));
        Assert.That(capturedBook.TotalCopies, Is.EqualTo(command.TotalCopies));
        Assert.That(capturedBook.AvailableCopies, Is.EqualTo(command.TotalCopies));
        Assert.That(capturedBook.IsActive, Is.True);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockBookRepository.Verify(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var command = new CreateBookCommand
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            CategoryId = Guid.NewGuid(),
            ISBN = "978-0132350884",
            PublishedYear = 2008,
            Publisher = "Prentice Hall",
            Description = "A book about writing clean code",
            TotalCopies = 10
        };

        _mockBookRepository.Setup(repo => repo.CreateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()))
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
        Assert.Throws<ArgumentNullException>(() => new CreateBookCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public async Task Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateBookCommandHandler(_mockUnitOfWork.Object, null));
    }
}
