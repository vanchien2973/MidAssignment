using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;

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
            var requestorId = 1;
            var requestDate = DateTime.UtcNow;
            var status = BorrowingRequestStatus.Waiting;
            var notes = "Test borrowing request";
            var approvalDate = DateTime.UtcNow.AddHours(2);
            var approverId = 2;

            // Act
            var request = new BookBorrowingRequest
            {
                RequestId = requestId,
                RequestorId = requestorId,
                RequestDate = requestDate,
                Status = status,
                Notes = notes,
                ApprovalDate = approvalDate,
                ApproverId = approverId
            };

            // Assert
            Assert.That(request.RequestId, Is.EqualTo(requestId));
            Assert.That(request.RequestorId, Is.EqualTo(requestorId));
            Assert.That(request.RequestDate, Is.EqualTo(requestDate).Within(TimeSpan.FromSeconds(1)));
            Assert.That(request.Status, Is.EqualTo(status));
            Assert.That(request.Notes, Is.EqualTo(notes));
            Assert.That(request.ApprovalDate, Is.EqualTo(approvalDate).Within(TimeSpan.FromSeconds(1)));
            Assert.That(request.ApproverId, Is.EqualTo(approverId));
        }

        [Test]
        public void BookBorrowingRequest_NavigationProperties_ShouldBeInitializedToNull()
        {
            // Arrange & Act
            var request = new BookBorrowingRequest();

            // Assert
            Assert.That(request.Requestor, Is.Null);
            Assert.That(request.Approver, Is.Null);
            Assert.That(request.RequestDetails, Is.Null);
        }

        [Test]
        public void BookBorrowingRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var request = new BookBorrowingRequest();

            // Assert
            Assert.That(request.RequestId, Is.EqualTo(Guid.Empty));
            Assert.That(request.RequestorId, Is.EqualTo(0));
            Assert.That(request.Status, Is.EqualTo(default(BorrowingRequestStatus)));
            Assert.That(request.Notes, Is.Null);
            Assert.That(request.ApprovalDate, Is.Null);
            Assert.That(request.ApproverId, Is.Null);
        }

        [Test]
        public void BookBorrowingRequest_SetStatus_ShouldUpdateStatusCorrectly()
        {
            // Arrange
            var request = new BookBorrowingRequest();

            // Act & Assert
            request.Status = BorrowingRequestStatus.Waiting;
            Assert.That(request.Status, Is.EqualTo(BorrowingRequestStatus.Waiting));

            request.Status = BorrowingRequestStatus.Approved;
            Assert.That(request.Status, Is.EqualTo(BorrowingRequestStatus.Approved));

            request.Status = BorrowingRequestStatus.Rejected;
            Assert.That(request.Status, Is.EqualTo(BorrowingRequestStatus.Rejected));
        }

        [Test]
        public void BookBorrowingRequest_WithDetails_ShouldTrackDetailsCorrectly()
        {
            // Arrange
            var request = new BookBorrowingRequest
            {
                RequestId = Guid.NewGuid(),
                RequestorId = 1,
                RequestDate = DateTime.UtcNow,
                Status = BorrowingRequestStatus.Waiting
            };

            var details = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail
                {
                    DetailId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    BookId = Guid.NewGuid(),
                    Status = BorrowingDetailStatus.Borrowing
                },
                new BookBorrowingRequestDetail
                {
                    DetailId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    BookId = Guid.NewGuid(),
                    Status = BorrowingDetailStatus.Borrowing
                }
            };

            // Act
            request.RequestDetails = details;

            // Assert
            Assert.That(request.RequestDetails, Is.Not.Null);
            Assert.That(request.RequestDetails.Count, Is.EqualTo(2));
            Assert.That(request.RequestDetails, Is.EquivalentTo(details));
        }

        [Test]
        public void BookBorrowingRequest_PropertiesInitialization_PropertiesHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var borrowingRequest = new BookBorrowingRequest();
            
            // Assert
            Assert.That(borrowingRequest.RequestId, Is.EqualTo(Guid.Empty));
            Assert.That(borrowingRequest.RequestorId, Is.EqualTo(0));
            Assert.That(borrowingRequest.RequestDate, Is.EqualTo(default(DateTime)));
            Assert.That(borrowingRequest.Status, Is.EqualTo(default(BorrowingRequestStatus)));
            Assert.That(borrowingRequest.ApproverId, Is.Null);
            Assert.That(borrowingRequest.ApprovalDate, Is.Null);
            Assert.That(borrowingRequest.Notes, Is.Null);
        }
        
        [Test]
        public void BookBorrowingRequest_SetProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var borrowingRequest = new BookBorrowingRequest();
            var requestId = Guid.NewGuid();
            var requestorId = 1;
            var requestDate = DateTime.UtcNow.AddDays(-1);
            var status = BorrowingRequestStatus.Approved;
            var approverId = 2;
            var approvalDate = DateTime.UtcNow;
            var notes = "Approved for 14 days";
            
            // Act
            borrowingRequest.RequestId = requestId;
            borrowingRequest.RequestorId = requestorId;
            borrowingRequest.RequestDate = requestDate;
            borrowingRequest.Status = status;
            borrowingRequest.ApproverId = approverId;
            borrowingRequest.ApprovalDate = approvalDate;
            borrowingRequest.Notes = notes;
            
            // Assert
            Assert.That(borrowingRequest.RequestId, Is.EqualTo(requestId));
            Assert.That(borrowingRequest.RequestorId, Is.EqualTo(requestorId));
            Assert.That(borrowingRequest.RequestDate, Is.EqualTo(requestDate).Within(TimeSpan.FromSeconds(1)));
            Assert.That(borrowingRequest.Status, Is.EqualTo(status));
            Assert.That(borrowingRequest.ApproverId, Is.EqualTo(approverId));
            Assert.That(borrowingRequest.ApprovalDate, Is.EqualTo(approvalDate).Within(TimeSpan.FromSeconds(1)));
            Assert.That(borrowingRequest.Notes, Is.EqualTo(notes));
        }
        
        [Test]
        public void BookBorrowingRequest_NavigationProperties_InitializedAsNull()
        {
            // Arrange & Act
            var borrowingRequest = new BookBorrowingRequest();
            
            // Assert
            Assert.That(borrowingRequest.Requestor, Is.Null);
            Assert.That(borrowingRequest.Approver, Is.Null);
            Assert.That(borrowingRequest.RequestDetails, Is.Null);
        }
        
        [Test]
        public void BookBorrowingRequest_SetNavigationProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var borrowingRequest = new BookBorrowingRequest();
            var requestor = new User { UserId = 1, Username = "requestor" };
            var approver = new User { UserId = 2, Username = "approver" };
            var requestDetails = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail { DetailId = Guid.NewGuid(), RequestId = borrowingRequest.RequestId },
                new BookBorrowingRequestDetail { DetailId = Guid.NewGuid(), RequestId = borrowingRequest.RequestId }
            };
            
            // Act
            borrowingRequest.Requestor = requestor;
            borrowingRequest.Approver = approver;
            borrowingRequest.RequestDetails = requestDetails;

            // Assert
            Assert.That(borrowingRequest.Requestor, Is.SameAs(requestor));
            Assert.That(borrowingRequest.Approver, Is.SameAs(approver));
            Assert.That(borrowingRequest.RequestDetails, Is.SameAs(requestDetails));
            Assert.That(borrowingRequest.RequestDetails.Count, Is.EqualTo(2));
        }
        
        [Test]
        public void BookBorrowingRequest_EnumValues_CorrectEnumValuesAreUsed()
        {
            // Arrange
            var borrowingRequest = new BookBorrowingRequest();
            
            // Act & Assert
            borrowingRequest.Status = BorrowingRequestStatus.Waiting;
            Assert.That(borrowingRequest.Status, Is.EqualTo(BorrowingRequestStatus.Waiting));
            
            borrowingRequest.Status = BorrowingRequestStatus.Approved;
            Assert.That(borrowingRequest.Status, Is.EqualTo(BorrowingRequestStatus.Approved));
            
            borrowingRequest.Status = BorrowingRequestStatus.Rejected;
            Assert.That(borrowingRequest.Status, Is.EqualTo(BorrowingRequestStatus.Rejected));
        }
    }
} 