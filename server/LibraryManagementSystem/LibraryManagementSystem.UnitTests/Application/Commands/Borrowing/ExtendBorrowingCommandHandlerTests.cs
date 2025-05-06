using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Borrowing;

public class ExtendBorrowingCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<ExtendBorrowingCommandHandler>> _mockLogger;
    private Mock<IBookBorrowingRequestDetailRepository> _mockDetailRepository;
    private ExtendBorrowingCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ExtendBorrowingCommandHandler>>();
        _mockDetailRepository = new Mock<IBookBorrowingRequestDetailRepository>();

        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequestDetails).Returns(_mockDetailRepository.Object);

        _handler = new ExtendBorrowingCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WithValidExtensionRequest_ReturnsTrue()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var currentDueDate = now.AddDays(2);
        var newDueDate = currentDueDate.AddDays(5); // 5 days extension, less than max 7 days
        
        var command = new ExtendBorrowingCommand
        {
            DetailId = detailId,
            UserId = 1,
            NewDueDate = newDueDate,
            Notes = "Extension requested"
        };

        var borrowingDetail = new BookBorrowingRequestDetail
        {
            DetailId = detailId,
            RequestId = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            Status = BorrowingDetailStatus.Borrowing,
            DueDate = currentDueDate,
            ExtensionDate = null
        };

        _mockDetailRepository.Setup(repo => repo.GetByIdAsync(detailId))
            .ReturnsAsync(borrowingDetail);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify the detail was updated
        Assert.That(borrowingDetail.Status, Is.EqualTo(BorrowingDetailStatus.Extended));
        // Assert.That(borrowingDetail.DueDate, Is.EqualTo(newDueDate).Within(TimeSpan.FromSeconds(1)));
        Assert.That(borrowingDetail.ExtensionDate, Is.Not.Null);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockDetailRepository.Verify(repo => repo.UpdateAsync(borrowingDetail), Times.Once);
    }

    [Test]
    public async Task Handle_WithNullDetail_ReturnsFalse()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var command = new ExtendBorrowingCommand
        {
            DetailId = detailId,
            UserId = 1,
            NewDueDate = now.AddDays(5)
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
    }

    [Test]
    public async Task Handle_WithDetailNotInBorrowingStatus_ReturnsFalse()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var command = new ExtendBorrowingCommand
        {
            DetailId = detailId,
            UserId = 1,
            NewDueDate = now.AddDays(5)
        };

        var borrowingDetail = new BookBorrowingRequestDetail
        {
            DetailId = detailId,
            Status = BorrowingDetailStatus.Returned // Already returned
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
    }

    [Test]
    public async Task Handle_WithAlreadyExtendedDetail_ReturnsFalse()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var command = new ExtendBorrowingCommand
        {
            DetailId = detailId,
            UserId = 1,
            NewDueDate = now.AddDays(5)
        };

        var borrowingDetail = new BookBorrowingRequestDetail
        {
            DetailId = detailId,
            Status = BorrowingDetailStatus.Borrowing,
            ExtensionDate = now.AddDays(-1) // Already extended
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
    }

    [Test]
    public async Task Handle_WithExtensionTooLong_ReturnsFalse()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var currentDueDate = now.AddDays(2);
        var newDueDate = currentDueDate.AddDays(10); // 10 days extension, more than max 7 days
        
        var command = new ExtendBorrowingCommand
        {
            DetailId = detailId,
            UserId = 1,
            NewDueDate = newDueDate
        };

        var borrowingDetail = new BookBorrowingRequestDetail
        {
            DetailId = detailId,
            Status = BorrowingDetailStatus.Borrowing,
            DueDate = currentDueDate,
            ExtensionDate = null
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
    }

    [Test]
    public async Task Handle_WithNoDueDate_ReturnsFalse()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var command = new ExtendBorrowingCommand
        {
            DetailId = detailId,
            UserId = 1,
            NewDueDate = now.AddDays(5)
        };

        var borrowingDetail = new BookBorrowingRequestDetail
        {
            DetailId = detailId,
            Status = BorrowingDetailStatus.Borrowing,
            DueDate = null, // No due date
            ExtensionDate = null
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
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var detailId = Guid.NewGuid();
        var command = new ExtendBorrowingCommand { DetailId = detailId };

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
        Assert.Throws<ArgumentNullException>(() => new ExtendBorrowingCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public void Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ExtendBorrowingCommandHandler(_mockUnitOfWork.Object, null));
    }
}
