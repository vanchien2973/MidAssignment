using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Borrowing;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Borrowing;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Borrowing
{
    [TestFixture]
    public class GetBorrowingRequestByIdQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IBookBorrowingRequestRepository> _mockBorrowingRequestRepository;
        private Mock<ILogger<GetBorrowingRequestByIdQueryHandler>> _mockLogger;
        private GetBorrowingRequestByIdQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockBorrowingRequestRepository = new Mock<IBookBorrowingRequestRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.BookBorrowingRequests).Returns(_mockBorrowingRequestRepository.Object);
            
            _mockLogger = new Mock<ILogger<GetBorrowingRequestByIdQueryHandler>>();
            
            _handler = new GetBorrowingRequestByIdQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WhenRequestExists_ShouldReturnMappedDto()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var requestorId = 1;
            var requestDate = DateTime.UtcNow;
            var status = BorrowingRequestStatus.Waiting;
            var notes = "Test borrowing request";
            
            var request = new BookBorrowingRequest
            {
                RequestId = requestId,
                RequestorId = requestorId,
                RequestDate = requestDate,
                Status = status,
                Notes = notes,
                RequestDetails = new List<BookBorrowingRequestDetail>
                {
                    new BookBorrowingRequestDetail
                    {
                        DetailId = Guid.NewGuid(),
                        RequestId = requestId,
                        BookId = Guid.NewGuid(),
                        Status = BorrowingDetailStatus.Borrowing
                    },
                    new BookBorrowingRequestDetail
                    {
                        DetailId = Guid.NewGuid(),
                        RequestId = requestId,
                        BookId = Guid.NewGuid(),
                        Status = BorrowingDetailStatus.Borrowing
                    }
                }
            };

            _mockBorrowingRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            var query = new GetBorrowingRequestByIdQuery { RequestId = requestId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.RequestId, Is.EqualTo(requestId));
            Assert.That(result.RequestorId, Is.EqualTo(requestorId));
            Assert.That(result.RequestDate, Is.EqualTo(requestDate));
            Assert.That(result.Status, Is.EqualTo(status));
            Assert.That(result.Notes, Is.EqualTo(notes));
            Assert.That(result.Details, Is.Not.Null);
            Assert.That(result.Details.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Handle_WhenRequestDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            
            _mockBorrowingRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync((BookBorrowingRequest)null);

            var query = new GetBorrowingRequestByIdQuery { RequestId = requestId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Handle_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            
            _mockBorrowingRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
                .ThrowsAsync(new Exception("Database error"));

            var query = new GetBorrowingRequestByIdQuery { RequestId = requestId };

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(query, CancellationToken.None));
        }
    }
} 