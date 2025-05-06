using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.UnitTests.Domain.Entities
{
    [TestFixture]
    public class BookBorrowingRequestDetailTests
    {
        [Test]
        public void BookBorrowingRequestDetail_PropertiesInitialization_PropertiesHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var detail = new BookBorrowingRequestDetail();
            
            // Assert
            Assert.That(detail.DetailId, Is.EqualTo(Guid.Empty));
            Assert.That(detail.RequestId, Is.EqualTo(Guid.Empty));
            Assert.That(detail.BookId, Is.EqualTo(Guid.Empty));
            Assert.That(detail.DueDate, Is.Null);
            Assert.That(detail.ReturnDate, Is.Null);
            Assert.That(detail.Status, Is.EqualTo(default(BorrowingDetailStatus)));
            Assert.That(detail.ExtensionDate, Is.Null);
            Assert.That(detail.Request, Is.Null);
            Assert.That(detail.Book, Is.Null);
        }
        
        [Test]
        public void BookBorrowingRequestDetail_SetProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var detail = new BookBorrowingRequestDetail();
            var detailId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var dueDate = now.AddDays(14);
            var returnDate = now.AddDays(10);
            var status = BorrowingDetailStatus.Borrowing;
            var extensionDate = now.AddDays(7);
            
            // Act
            detail.DetailId = detailId;
            detail.RequestId = requestId;
            detail.BookId = bookId;
            detail.DueDate = dueDate;
            detail.ReturnDate = returnDate;
            detail.Status = status;
            detail.ExtensionDate = extensionDate;
            
            // Assert
            Assert.That(detail.DetailId, Is.EqualTo(detailId));
            Assert.That(detail.RequestId, Is.EqualTo(requestId));
            Assert.That(detail.BookId, Is.EqualTo(bookId));
            // Assert.That(detail.DueDate, Is.EqualTo(dueDate).Within(TimeSpan.FromSeconds(1)));
            // Assert.That(detail.ReturnDate, Is.EqualTo(returnDate).Within(TimeSpan.FromSeconds(1)));
            Assert.That(detail.Status, Is.EqualTo(status));
            // Assert.That(detail.ExtensionDate, Is.EqualTo(extensionDate).Within(TimeSpan.FromSeconds(1)));
        }
        
        [Test]
        public void BookBorrowingRequestDetail_NavigationProperties_InitializedAsNull()
        {
            // Arrange & Act
            var detail = new BookBorrowingRequestDetail();
            
            // Assert
            Assert.That(detail.Request, Is.Null);
            Assert.That(detail.Book, Is.Null);
        }
        
        [Test]
        public void BookBorrowingRequestDetail_SetNavigationProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var detail = new BookBorrowingRequestDetail();
            var request = new BookBorrowingRequest 
            { 
                RequestId = Guid.NewGuid(), 
                RequestorId = 1 
            };
            var book = new Book 
            { 
                BookId = Guid.NewGuid(), 
                Title = "Design Patterns" 
            };
            
            // Act
            detail.Request = request;
            detail.Book = book;
            
            // Assert
            Assert.That(detail.Request, Is.SameAs(request));
            Assert.That(detail.Book, Is.SameAs(book));
            Assert.That(detail.Book.Title, Is.EqualTo("Design Patterns"));
        }
        
        [Test]
        public void BookBorrowingRequestDetail_EnumValues_CorrectValues()
        {
            // Arrange & Act
            var borrowing = BorrowingDetailStatus.Borrowing;
            var returned = BorrowingDetailStatus.Returned;
            var extended = BorrowingDetailStatus.Extended;
            
            // Assert
            Assert.That((int)borrowing, Is.EqualTo(1));
            Assert.That((int)returned, Is.EqualTo(2));
            Assert.That((int)extended, Is.EqualTo(3));
        }
        
        [Test]
        public void BookBorrowingRequestDetail_StatusChanges_StatusChangesCorrectly()
        {
            // Arrange
            var detail = new BookBorrowingRequestDetail
            {
                Status = BorrowingDetailStatus.Borrowing
            };
            
            // Act & Assert - Change to Returned
            detail.Status = BorrowingDetailStatus.Returned;
            Assert.That(detail.Status, Is.EqualTo(BorrowingDetailStatus.Returned));
            
            // Act & Assert - Change to Extended
            detail.Status = BorrowingDetailStatus.Extended;
            Assert.That(detail.Status, Is.EqualTo(BorrowingDetailStatus.Extended));
        }
    }
} 