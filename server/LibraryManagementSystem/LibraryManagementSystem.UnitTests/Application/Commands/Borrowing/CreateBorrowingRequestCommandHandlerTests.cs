using System;
using System.Collections.Generic;
using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Borrowing;

public class CreateBorrowingRequestCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<CreateBorrowingRequestCommandHandler>> _mockLogger;
    private Mock<IBookBorrowingRequestRepository> _mockBookBorrowingRequestRepository;
    private CreateBorrowingRequestCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<CreateBorrowingRequestCommandHandler>>();
        _mockBookBorrowingRequestRepository = new Mock<IBookBorrowingRequestRepository>();

        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequests).Returns(_mockBookBorrowingRequestRepository.Object);

        _handler = new CreateBorrowingRequestCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidRequest_ReturnsRequestId()
    {
        // Arrange
        var command = new CreateBorrowingRequestCommand
        {
            RequestorId = 1,
            Notes = "Test borrowing request",
            Books = new List<BorrowingBookItem>
            {
                new BorrowingBookItem { BookId = Guid.NewGuid() },
                new BorrowingBookItem { BookId = Guid.NewGuid() }
            }
        };

        BookBorrowingRequest capturedRequest = null;

        _mockBookBorrowingRequestRepository.Setup(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()))
            .Callback<BookBorrowingRequest>(request => capturedRequest = request)
            .ReturnsAsync((BookBorrowingRequest request) => request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
        
        Assert.That(capturedRequest, Is.Not.Null);
        Assert.That(capturedRequest.RequestorId, Is.EqualTo(command.RequestorId));
        Assert.That(capturedRequest.Notes, Is.EqualTo(command.Notes));
        Assert.That(capturedRequest.Status, Is.EqualTo(BorrowingRequestStatus.Waiting));
        Assert.That(capturedRequest.RequestDetails.Count, Is.EqualTo(command.Books.Count));
        
        foreach (var bookItem in command.Books)
        {
            Assert.That(capturedRequest.RequestDetails.Any(d => d.BookId == bookItem.BookId), Is.True);
        }
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockBookBorrowingRequestRepository.Verify(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithEmptyBooksList_CreatesRequestWithNoDetails()
    {
        // Arrange
        var command = new CreateBorrowingRequestCommand
        {
            RequestorId = 1,
            Notes = "Test borrowing request",
            Books = new List<BorrowingBookItem>()
        };

        BookBorrowingRequest capturedRequest = null;

        _mockBookBorrowingRequestRepository.Setup(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()))
            .Callback<BookBorrowingRequest>(request => capturedRequest = request)
            .ReturnsAsync((BookBorrowingRequest request) => request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
        
        Assert.That(capturedRequest, Is.Not.Null);
        Assert.That(capturedRequest.RequestDetails.Count, Is.EqualTo(0));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ThrowsException()
    {
        // Arrange
        var command = new CreateBorrowingRequestCommand
        {
            RequestorId = 1,
            Books = new List<BorrowingBookItem>
            {
                new BorrowingBookItem { BookId = Guid.NewGuid() }
            }
        };

        _mockBookBorrowingRequestRepository.Setup(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
    }

    [Test]
    public void Handle_WithNullUnitOfWork_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateBorrowingRequestCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public void Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateBorrowingRequestCommandHandler(_mockUnitOfWork.Object, null));
    }
}
