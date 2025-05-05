using System;
using System.Collections.Generic;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Domain.Entities
{
    [TestFixture]
    public class BookBorrowingRequestTests
    {
        [Test]
        public void BookBorrowingRequest_InitializeWithValidProperties_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var requestDate = DateTime.UtcNow;
            var status = RequestStatus.Pending;
            var notes = "Test borrowing request";
            var processedDate = DateTime.UtcNow.AddHours(2);
            var processedByUserId = Guid.NewGuid();

            // Act
            var request = new BookBorrowingRequest
            {
                RequestId = requestId,
                UserId = userId,
                RequestDate = requestDate,
                Status = status,
                Notes = notes,
                ProcessedDate = processedDate,
                ProcessedByUserId = processedByUserId
            };

            // Assert
            Assert.That(request.RequestId, Is.EqualTo(requestId));
            Assert.That(request.UserId, Is.EqualTo(userId));
            Assert.That(request.RequestDate, Is.EqualTo(requestDate));
            Assert.That(request.Status, Is.EqualTo(status));
            Assert.That(request.Notes, Is.EqualTo(notes));
            Assert.That(request.ProcessedDate, Is.EqualTo(processedDate));
            Assert.That(request.ProcessedByUserId, Is.EqualTo(processedByUserId));
        }

        [Test]
        public void BookBorrowingRequest_NavigationProperties_ShouldBeInitializedToNull()
        {
            // Arrange & Act
            var request = new BookBorrowingRequest();

            // Assert
            Assert.That(request.User, Is.Null);
            Assert.That(request.ProcessedByUser, Is.Null);
            Assert.That(request.Details, Is.Null);
        }

        [Test]
        public void BookBorrowingRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var request = new BookBorrowingRequest();

            // Assert
            Assert.That(request.RequestId, Is.EqualTo(Guid.Empty));
            Assert.That(request.UserId, Is.EqualTo(Guid.Empty));
            Assert.That(request.Status, Is.EqualTo(RequestStatus.Pending)); // Default enum value
            Assert.That(request.Notes, Is.Null);
            Assert.That(request.ProcessedDate, Is.Null);
            Assert.That(request.ProcessedByUserId, Is.Null);
        }

        [Test]
        public void BookBorrowingRequest_SetStatus_ShouldUpdateStatusCorrectly()
        {
            // Arrange
            var request = new BookBorrowingRequest();

            // Act & Assert
            request.Status = RequestStatus.Pending;
            Assert.That(request.Status, Is.EqualTo(RequestStatus.Pending));

            request.Status = RequestStatus.Approved;
            Assert.That(request.Status, Is.EqualTo(RequestStatus.Approved));

            request.Status = RequestStatus.Rejected;
            Assert.That(request.Status, Is.EqualTo(RequestStatus.Rejected));

            request.Status = RequestStatus.Completed;
            Assert.That(request.Status, Is.EqualTo(RequestStatus.Completed));
        }

        [Test]
        public void BookBorrowingRequest_WithDetails_ShouldTrackDetailsCorrectly()
        {
            // Arrange
            var request = new BookBorrowingRequest
            {
                RequestId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                RequestDate = DateTime.UtcNow,
                Status = RequestStatus.Pending
            };

            var details = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail
                {
                    DetailId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    BookId = Guid.NewGuid(),
                    Status = BookStatus.Borrowed
                },
                new BookBorrowingRequestDetail
                {
                    DetailId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    BookId = Guid.NewGuid(),
                    Status = BookStatus.Borrowed
                }
            };

            // Act
            request.Details = details;

            // Assert
            Assert.That(request.Details, Is.Not.Null);
            Assert.That(request.Details.Count, Is.EqualTo(2));
            Assert.That(request.Details, Is.EquivalentTo(details));
        }
    }
} 