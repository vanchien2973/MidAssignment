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
    public class UpdateBookCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IBookRepository> _mockBookRepository;
        private Mock<ILogger<UpdateBookCommandHandler>> _mockLogger;
        private UpdateBookCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockBookRepository = new Mock<IBookRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);
            
            _mockLogger = new Mock<ILogger<UpdateBookCommandHandler>>();
            
            _handler = new UpdateBookCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WhenBookExists_ShouldUpdateBookAndReturnTrue()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var command = new UpdateBookCommand
            {
                BookId = bookId,
                Title = "Updated Book",
                Author = "Updated Author",
                CategoryId = Guid.NewGuid(),
                ISBN = "9876543210",
                PublishedYear = 2023,
                Publisher = "Updated Publisher",
                Description = "Updated Description",
                TotalCopies = 15
            };

            var existingBook = new Domain.Entities.Book
            {
                BookId = bookId,
                Title = "Original Book",
                Author = "Original Author",
                CategoryId = Guid.NewGuid(),
                ISBN = "1234567890",
                PublishedYear = 2020,
                Publisher = "Original Publisher",
                Description = "Original Description",
                TotalCopies = 10,
                AvailableCopies = 5,
                IsActive = true
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(existingBook);
            
            _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Book>()))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
            _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Book>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WhenBookDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var command = new UpdateBookCommand
            {
                BookId = bookId,
                Title = "Updated Book",
                Author = "Updated Author",
                CategoryId = Guid.NewGuid(),
                ISBN = "9876543210",
                PublishedYear = 2023,
                Publisher = "Updated Publisher",
                Description = "Updated Description",
                TotalCopies = 15
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
            _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Book>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WhenExceptionThrown_ShouldRollbackTransactionAndReturnFalse()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var command = new UpdateBookCommand
            {
                BookId = bookId,
                Title = "Updated Book",
                Author = "Updated Author",
                CategoryId = Guid.NewGuid(),
                ISBN = "9876543210",
                PublishedYear = 2023,
                Publisher = "Updated Publisher",
                Description = "Updated Description",
                TotalCopies = 15
            };

            var existingBook = new Domain.Entities.Book
            {
                BookId = bookId,
                Title = "Original Book",
                Author = "Original Author"
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(existingBook);
            
            _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Book>()))
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