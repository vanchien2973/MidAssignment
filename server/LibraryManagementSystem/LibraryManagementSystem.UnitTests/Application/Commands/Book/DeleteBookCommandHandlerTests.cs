using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Book
{
    [TestFixture]
    public class DeleteBookCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IBookRepository> _mockBookRepository;
        private Mock<IBookBorrowingRequestDetailRepository> _mockBorrowingDetailRepository;
        private Mock<ILogger<DeleteBookCommandHandler>> _mockLogger;
        private DeleteBookCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockBookRepository = new Mock<IBookRepository>();
            _mockBorrowingDetailRepository = new Mock<IBookBorrowingRequestDetailRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequestDetails).Returns(_mockBorrowingDetailRepository.Object);
            
            _mockLogger = new Mock<ILogger<DeleteBookCommandHandler>>();
            
            _handler = new DeleteBookCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WhenBookExistsAndNoBorrowings_ShouldDeleteBookAndReturnTrue()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var command = new DeleteBookCommand
            {
                BookId = bookId
            };

            var existingBook = new Domain.Entities.Book
            {
                BookId = bookId,
                Title = "Test Book",
                Author = "Test Author"
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(existingBook);
            
            _mockBorrowingDetailRepository.Setup(repo => repo.HasActiveBorrowingsForBookAsync(bookId))
                .ReturnsAsync(false);
            
            _mockBookRepository.Setup(repo => repo.DeleteAsync(bookId))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
            _mockBorrowingDetailRepository.Verify(repo => repo.HasActiveBorrowingsForBookAsync(bookId), Times.Once);
            _mockBookRepository.Verify(repo => repo.DeleteAsync(bookId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WhenBookDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var command = new DeleteBookCommand
            {
                BookId = bookId
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync((Domain.Entities.Book)null);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
            _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
            _mockBorrowingDetailRepository.Verify(repo => repo.HasActiveBorrowingsForBookAsync(bookId), Times.Never);
            _mockBookRepository.Verify(repo => repo.DeleteAsync(bookId), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WhenBookHasActiveBorrowings_ShouldReturnFalse()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var command = new DeleteBookCommand
            {
                BookId = bookId
            };

            var existingBook = new Domain.Entities.Book
            {
                BookId = bookId,
                Title = "Test Book",
                Author = "Test Author"
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(existingBook);
            
            _mockBorrowingDetailRepository.Setup(repo => repo.HasActiveBorrowingsForBookAsync(bookId))
                .ReturnsAsync(true);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
            _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
            _mockBorrowingDetailRepository.Verify(repo => repo.HasActiveBorrowingsForBookAsync(bookId), Times.Once);
            _mockBookRepository.Verify(repo => repo.DeleteAsync(bookId), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WhenExceptionThrown_ShouldRollbackTransactionAndReturnFalse()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var command = new DeleteBookCommand
            {
                BookId = bookId
            };

            var existingBook = new Domain.Entities.Book
            {
                BookId = bookId,
                Title = "Test Book",
                Author = "Test Author"
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(existingBook);
            
            _mockBorrowingDetailRepository.Setup(repo => repo.HasActiveBorrowingsForBookAsync(bookId))
                .ReturnsAsync(false);
            
            _mockBookRepository.Setup(repo => repo.DeleteAsync(bookId))
                .ThrowsAsync(new Exception("Database error"));
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }
    }
} 