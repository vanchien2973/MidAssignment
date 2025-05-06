using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Borrowing;

public class UpdateBorrowingRequestStatusCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<UpdateBorrowingRequestStatusCommandHandler>> _mockLogger;
    private Mock<IBookBorrowingRequestRepository> _mockRequestRepository;
    private Mock<IBookRepository> _mockBookRepository;
    private UpdateBorrowingRequestStatusCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateBorrowingRequestStatusCommandHandler>>();
        _mockRequestRepository = new Mock<IBookBorrowingRequestRepository>();
        _mockBookRepository = new Mock<IBookRepository>();

        _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequests).Returns(_mockRequestRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);

        _handler = new UpdateBorrowingRequestStatusCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WhenApprovingRequest_ReturnsTrue()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var bookId1 = Guid.NewGuid();
        var bookId2 = Guid.NewGuid();
        var command = new UpdateBorrowingRequestStatusCommand
        {
            RequestId = requestId,
            ApproverId = 1,
            Status = BorrowingRequestStatus.Approved,
            Notes = "Approved by admin",
            DueDays = 14
        };

        var book1 = new LibraryManagementSystem.Domain.Entities.Book { BookId = bookId1, AvailableCopies = 5 };
        var book2 = new LibraryManagementSystem.Domain.Entities.Book { BookId = bookId2, AvailableCopies = 3 };

        var borrowingRequest = new BookBorrowingRequest
        {
            RequestId = requestId,
            RequestorId = 2,
            Status = BorrowingRequestStatus.Waiting,
            RequestDetails = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail { BookId = bookId1 },
                new BookBorrowingRequestDetail { BookId = bookId2 }
            }
        };

        _mockRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
            .ReturnsAsync(borrowingRequest);
        
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId1))
            .ReturnsAsync(book1);
        
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId2))
            .ReturnsAsync(book2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        Assert.That(borrowingRequest.Status, Is.EqualTo(BorrowingRequestStatus.Approved));
        Assert.That(borrowingRequest.ApproverId, Is.EqualTo(command.ApproverId));
        Assert.That(borrowingRequest.ApprovalDate, Is.Not.Null);
        Assert.That(borrowingRequest.Notes, Is.EqualTo(command.Notes));
        
        // Check due dates were set
        foreach (var detail in borrowingRequest.RequestDetails)
        {
            Assert.That(detail.DueDate, Is.Not.Null);
            var expectedDueDate = DateTime.UtcNow.AddDays(command.DueDays.Value).Date;
            Assert.That(detail.DueDate.Value.Date, Is.EqualTo(expectedDueDate));
        }
        
        // Check book available copies were updated
        Assert.That(book1.AvailableCopies, Is.EqualTo(4));
        Assert.That(book2.AvailableCopies, Is.EqualTo(2));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockRequestRepository.Verify(repo => repo.UpdateAsync(borrowingRequest), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()), Times.Exactly(2));
    }

    [Test]
    public async Task Handle_WhenRejectingRequest_ReturnsTrue()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new UpdateBorrowingRequestStatusCommand
        {
            RequestId = requestId,
            ApproverId = 1,
            Status = BorrowingRequestStatus.Rejected,
            Notes = "Rejected due to overdue books"
        };

        var borrowingRequest = new BookBorrowingRequest
        {
            RequestId = requestId,
            RequestorId = 2,
            Status = BorrowingRequestStatus.Waiting,
            RequestDetails = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail { BookId = Guid.NewGuid() },
                new BookBorrowingRequestDetail { BookId = Guid.NewGuid() }
            }
        };

        _mockRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
            .ReturnsAsync(borrowingRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        Assert.That(borrowingRequest.Status, Is.EqualTo(BorrowingRequestStatus.Rejected));
        Assert.That(borrowingRequest.ApproverId, Is.EqualTo(command.ApproverId));
        Assert.That(borrowingRequest.ApprovalDate, Is.Not.Null);
        Assert.That(borrowingRequest.Notes, Is.EqualTo(command.Notes));
        
        // Due dates should not be set for rejected requests
        foreach (var detail in borrowingRequest.RequestDetails)
        {
            Assert.That(detail.DueDate, Is.Null);
        }
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockRequestRepository.Verify(repo => repo.UpdateAsync(borrowingRequest), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithDefaultDueDays_Sets14DaysAsDueDate()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new UpdateBorrowingRequestStatusCommand
        {
            RequestId = requestId,
            ApproverId = 1,
            Status = BorrowingRequestStatus.Approved,
            Notes = "Approved",
            DueDays = null // No due days specified, should default to 14
        };

        var book = new LibraryManagementSystem.Domain.Entities.Book { BookId = bookId, AvailableCopies = 5 };

        var borrowingRequest = new BookBorrowingRequest
        {
            RequestId = requestId,
            Status = BorrowingRequestStatus.Waiting,
            RequestDetails = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail { BookId = bookId }
            }
        };

        _mockRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
            .ReturnsAsync(borrowingRequest);
        
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        var expectedDueDate = DateTime.UtcNow.AddDays(14).Date;
        Assert.That(borrowingRequest.RequestDetails.First().DueDate.Value.Date, Is.EqualTo(expectedDueDate));
    }

    [Test]
    public async Task Handle_WithNullRequest_ReturnsFalse()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new UpdateBorrowingRequestStatusCommand
        {
            RequestId = requestId,
            ApproverId = 1,
            Status = BorrowingRequestStatus.Approved
        };

        _mockRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
            .ReturnsAsync((BookBorrowingRequest)null);
            
        // Đặt một flag để kiểm tra xem RollbackTransactionAsync có được gọi không
        bool rollbackCalled = false;
        _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync())
            .Callback(() => rollbackCalled = true)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(rollbackCalled, Is.True, "RollbackTransactionAsync should have been called");
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockRequestRepository.Verify(repo => repo.UpdateAsync(It.IsAny<BookBorrowingRequest>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithNonWaitingRequest_ReturnsFalse()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new UpdateBorrowingRequestStatusCommand
        {
            RequestId = requestId,
            ApproverId = 1,
            Status = BorrowingRequestStatus.Approved
        };

        var borrowingRequest = new BookBorrowingRequest
        {
            RequestId = requestId,
            Status = BorrowingRequestStatus.Approved // Already approved
        };

        _mockRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
            .ReturnsAsync(borrowingRequest);
            
        _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);
        
        // Đặt một flag để kiểm tra xem RollbackTransactionAsync có được gọi không
        bool rollbackCalled = false;
        _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync())
            .Callback(() => rollbackCalled = true)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(rollbackCalled, Is.True, "RollbackTransactionAsync should have been called");
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
        _mockRequestRepository.Verify(repo => repo.UpdateAsync(It.IsAny<BookBorrowingRequest>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithNullBook_SkipsAvailableCopiesUpdate()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new UpdateBorrowingRequestStatusCommand
        {
            RequestId = requestId,
            ApproverId = 1,
            Status = BorrowingRequestStatus.Approved
        };

        var borrowingRequest = new BookBorrowingRequest
        {
            RequestId = requestId,
            Status = BorrowingRequestStatus.Waiting,
            RequestDetails = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail { BookId = bookId }
            }
        };

        _mockRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
            .ReturnsAsync(borrowingRequest);
        
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((LibraryManagementSystem.Domain.Entities.Book)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockRequestRepository.Verify(repo => repo.UpdateAsync(borrowingRequest), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<LibraryManagementSystem.Domain.Entities.Book>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithUnavailableBook_SkipsAvailableCopiesUpdate()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new UpdateBorrowingRequestStatusCommand
        {
            RequestId = requestId,
            ApproverId = 1,
            Status = BorrowingRequestStatus.Approved
        };

        var book = new LibraryManagementSystem.Domain.Entities.Book { BookId = bookId, AvailableCopies = 0 }; // No available copies

        var borrowingRequest = new BookBorrowingRequest
        {
            RequestId = requestId,
            Status = BorrowingRequestStatus.Waiting,
            RequestDetails = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail { BookId = bookId }
            }
        };

        _mockRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
            .ReturnsAsync(borrowingRequest);
        
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // The available copies should not change
        Assert.That(book.AvailableCopies, Is.EqualTo(0));
        
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        _mockRequestRepository.Verify(repo => repo.UpdateAsync(borrowingRequest), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(book), Times.Never);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new UpdateBorrowingRequestStatusCommand { RequestId = requestId };

        _mockRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
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
        Assert.Throws<ArgumentNullException>(() => new UpdateBorrowingRequestStatusCommandHandler(null, _mockLogger.Object));
    }

    [Test]
    public void Handle_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UpdateBorrowingRequestStatusCommandHandler(_mockUnitOfWork.Object, null));
    }
}
