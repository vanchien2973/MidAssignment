using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Borrowing;

public class ReturnBookCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<ReturnBookCommandHandler>> _mockLogger;
    private Mock<IBookBorrowingRequestDetailRepository> _mockDetailRepository;
    private Mock<IBookRepository> _mockBookRepository;
    private ReturnBookCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ReturnBookCommandHandler>>();
        _mockDetailRepository = new Mock<IBookBorrowingRequestDetailRepository>();
        _mockBookRepository = new Mock<IBookRepository>();

        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequestDetails).Returns(_mockDetailRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);

        _handler = new ReturnBookCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidBorrowingDetail_ReturnsTrue()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new ReturnBookCommand
        {
            DetailId = detailId,
            UserId = 1,
            Notes = "Book returned in good condition"
        };

        var borrowingDetail = new BookBorrowingRequestDetail
        {
            DetailId = detailId,
            RequestId = Guid.NewGuid(),
            BookId = bookId,
            Status = BorrowingDetailStatus.Borrowing,
            DueDate = DateTime.UtcNow.AddDays(5),
            ReturnDate = null
        };

        var book = new LibraryManagementSystem.Domain.Entities.Book
        {
            BookId = bookId,
            Title = "Test Book",
            TotalCopies = 10,
            AvailableCopies = 5
        };

        _mockDetailRepository.Setup(repo => repo.GetByIdAsync(detailId))
            .ReturnsAsync(borrowingDetail);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify the detail was updated
        Assert.That(borrowingDetail.Status, Is.EqualTo(BorrowingDetailStatus.Returned));
        Assert.That(borrowingDetail.ReturnDate, Is.Not.Null);
        
        // Verify the book available copies was incremented
        Assert.That(book.AvailableCopies, Is.EqualTo(6));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockDetailRepository.Verify(repo => repo.UpdateAsync(borrowingDetail), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(book), Times.Once);
    }

    [Test]
    public async Task Handle_WithExtendedBorrowingDetail_ReturnsTrue()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new ReturnBookCommand
        {
            DetailId = detailId,
            UserId = 1
        };

        var borrowingDetail = new BookBorrowingRequestDetail
        {
            DetailId = detailId,
            RequestId = Guid.NewGuid(),
            BookId = bookId,
            Status = BorrowingDetailStatus.Extended,
            DueDate = DateTime.UtcNow.AddDays(5),
            ExtensionDate = DateTime.UtcNow.AddDays(-2),
            ReturnDate = null
        };

        var book = new LibraryManagementSystem.Domain.Entities.Book
        {
            BookId = bookId,
            Title = "Test Book",
            TotalCopies = 10,
            AvailableCopies = 5
        };

        _mockDetailRepository.Setup(repo => repo.GetByIdAsync(detailId))
            .ReturnsAsync(borrowingDetail);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify the detail was updated
        Assert.That(borrowingDetail.Status, Is.EqualTo(BorrowingDetailStatus.Returned));
        Assert.That(borrowingDetail.ReturnDate, Is.Not.Null);
        
        // Verify the book available copies was incremented
        Assert.That(book.AvailableCopies, Is.EqualTo(6));
    }

    [Test]
    public async Task Handle_WithNullDetail_ReturnsFalse()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var command = new ReturnBookCommand
        {
            DetailId = detailId,
            UserId = 1
        };

        _mockDetailRepository.Setup(repo => repo.GetByIdAsync(detailId))
            .ReturnsAsync((BookBorrowingRequestDetail)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockDetailRepository.Verify(repo => repo.UpdateAsync(It.IsAny<BookBorrowingRequestDetail>()), Times.Never);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithAlreadyReturnedDetail_ReturnsFalse()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var command = new ReturnBookCommand
        {
            DetailId = detailId,
            UserId = 1
        };

        var borrowingDetail = new BookBorrowingRequestDetail
        {
            DetailId = detailId,
            Status = BorrowingDetailStatus.Returned, // Already returned
            ReturnDate = DateTime.UtcNow.AddDays(-1)
        };

        _mockDetailRepository.Setup(repo => repo.GetByIdAsync(detailId))
            .ReturnsAsync(borrowingDetail);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockDetailRepository.Verify(repo => repo.UpdateAsync(It.IsAny<BookBorrowingRequestDetail>()), Times.Never);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithNullBook_StillReturnsTrue()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new ReturnBookCommand
        {
            DetailId = detailId,
            UserId = 1
        };

        var borrowingDetail = new BookBorrowingRequestDetail
        {
            DetailId = detailId,
            RequestId = Guid.NewGuid(),
            BookId = bookId,
            Status = BorrowingDetailStatus.Borrowing
        };

        _mockDetailRepository.Setup(repo => repo.GetByIdAsync(detailId))
            .ReturnsAsync(borrowingDetail);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Book)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify the detail was updated
        Assert.That(borrowingDetail.Status, Is.EqualTo(BorrowingDetailStatus.Returned));
        Assert.That(borrowingDetail.ReturnDate, Is.Not.Null);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockDetailRepository.Verify(repo => repo.UpdateAsync(borrowingDetail), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var command = new ReturnBookCommand { DetailId = detailId };

        _mockDetailRepository.Setup(repo => repo.GetByIdAsync(detailId))
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
        Assert.Throws<ArgumentNullException>(() => new ReturnBookCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public void Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ReturnBookCommandHandler(_mockUnitOfWork.Object, null));
    }
}
