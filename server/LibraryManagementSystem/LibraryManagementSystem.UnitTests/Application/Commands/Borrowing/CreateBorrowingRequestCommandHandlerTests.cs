using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Borrowing
{
    [TestFixture]
    public class CreateBorrowingRequestCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IBookBorrowingRequestRepository> _mockBorrowingRequestRepository;
        private Mock<ILogger<CreateBorrowingRequestCommandHandler>> _mockLogger;
        private CreateBorrowingRequestCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockBorrowingRequestRepository = new Mock<IBookBorrowingRequestRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequests).Returns(_mockBorrowingRequestRepository.Object);
            
            _mockLogger = new Mock<ILogger<CreateBorrowingRequestCommandHandler>>();
            
            _handler = new CreateBorrowingRequestCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldCreateBorrowingRequestAndReturnId()
        {
            // Arrange
            var requestorId = 1;
            var notes = "Test borrowing request";
            var books = new List<BorrowingBookItem>
            {
                new BorrowingBookItem { BookId = Guid.NewGuid() },
                new BorrowingBookItem { BookId = Guid.NewGuid() }
            };

            var command = new CreateBorrowingRequestCommand
            {
                RequestorId = requestorId,
                Notes = notes,
                Books = books
            };

            _mockBorrowingRequestRepository.Setup(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.EqualTo(Guid.Empty));
            
            _mockBorrowingRequestRepository.Verify(repo => repo.CreateAsync(It.Is<BookBorrowingRequest>(r => 
                r.RequestorId == requestorId && 
                r.Notes == notes && 
                r.Status == BorrowingRequestStatus.Waiting &&
                r.RequestDetails.Count == 2)), Times.Once);
            
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WithEmptyBooksList_ShouldCreateRequestWithNoDetails()
        {
            // Arrange
            var requestorId = 1;
            var notes = "Test borrowing request with no books";

            var command = new CreateBorrowingRequestCommand
            {
                RequestorId = requestorId,
                Notes = notes,
                Books = new List<BorrowingBookItem>()
            };

            _mockBorrowingRequestRepository.Setup(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.EqualTo(Guid.Empty));
            
            _mockBorrowingRequestRepository.Verify(repo => repo.CreateAsync(It.Is<BookBorrowingRequest>(r => 
                r.RequestorId == requestorId && 
                r.Notes == notes && 
                r.Status == BorrowingRequestStatus.Waiting &&
                r.RequestDetails.Count == 0)), Times.Once);
            
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public void Handle_WhenExceptionOccurs_ShouldRollbackTransactionAndThrowException()
        {
            // Arrange
            var requestorId = 1;
            var notes = "Test borrowing request";
            var books = new List<BorrowingBookItem>
            {
                new BorrowingBookItem { BookId = Guid.NewGuid() },
                new BorrowingBookItem { BookId = Guid.NewGuid() }
            };

            var command = new CreateBorrowingRequestCommand
            {
                RequestorId = requestorId,
                Notes = notes,
                Books = books
            };

            _mockBorrowingRequestRepository.Setup(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()))
                .ThrowsAsync(new Exception("Database error"));
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(command, CancellationToken.None));
            
            _mockBorrowingRequestRepository.Verify(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }
    }
} 