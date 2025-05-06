using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.UnitTests.Domain.Entities
{
    [TestFixture]
    public class BookTests
    {
        [Test]
        public void Book_PropertiesInitialization_PropertiesHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var book = new Book();
            
            // Assert
            Assert.That(book.BookId, Is.EqualTo(Guid.Empty));
            Assert.That(book.Title, Is.Null);
            Assert.That(book.Author, Is.Null);
            Assert.That(book.CategoryId, Is.EqualTo(Guid.Empty));
            Assert.That(book.ISBN, Is.Null);
            Assert.That(book.PublishedYear, Is.Null);
            Assert.That(book.Publisher, Is.Null);
            Assert.That(book.Description, Is.Null);
            Assert.That(book.TotalCopies, Is.EqualTo(0));
            Assert.That(book.AvailableCopies, Is.EqualTo(0));
            Assert.That(book.IsActive, Is.EqualTo(false));
        }
        
        [Test]
        public void Book_SetProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var book = new Book();
            var bookId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var title = "Clean Code";
            var author = "Robert C. Martin";
            var isbn = "9780132350884";
            var publishedYear = 2008;
            var publisher = "Prentice Hall";
            var description = "A book about clean code principles";
            var totalCopies = 10;
            var availableCopies = 5;
            var createdDate = DateTime.UtcNow;
            var isActive = true;

            // Act
            book.BookId = bookId;
            book.CategoryId = categoryId;
            book.Title = title;
            book.Author = author;
            book.ISBN = isbn;
            book.PublishedYear = publishedYear;
            book.Publisher = publisher;
            book.Description = description;
            book.TotalCopies = totalCopies;
            book.AvailableCopies = availableCopies;
            book.CreatedDate = createdDate;
            book.IsActive = isActive;

            // Assert
            Assert.That(book.BookId, Is.EqualTo(bookId));
            Assert.That(book.CategoryId, Is.EqualTo(categoryId));
            Assert.That(book.Title, Is.EqualTo(title));
            Assert.That(book.Author, Is.EqualTo(author));
            Assert.That(book.ISBN, Is.EqualTo(isbn));
            Assert.That(book.PublishedYear, Is.EqualTo(publishedYear));
            Assert.That(book.Publisher, Is.EqualTo(publisher));
            Assert.That(book.Description, Is.EqualTo(description));
            Assert.That(book.TotalCopies, Is.EqualTo(totalCopies));
            Assert.That(book.AvailableCopies, Is.EqualTo(availableCopies));
            Assert.That(book.CreatedDate, Is.EqualTo(createdDate));
            Assert.That(book.IsActive, Is.EqualTo(isActive));
        }

        [Test]
        public void Book_NavigationProperties_InitializedAsNull()
        {
            // Arrange & Act
            var book = new Book();

            // Assert
            Assert.That(book.Category, Is.Null);
            Assert.That(book.BorrowingDetails, Is.Null);
        }

        [Test]
        public void Book_SetNavigationProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var book = new Book();
            var category = new Category { CategoryId = Guid.NewGuid(), CategoryName = "Programming" };
            var borrowingDetails = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail { DetailId = Guid.NewGuid(), BookId = book.BookId },
                new BookBorrowingRequestDetail { DetailId = Guid.NewGuid(), BookId = book.BookId }
            };
            
            // Act
            book.Category = category;
            book.BorrowingDetails = borrowingDetails;

            // Assert
            Assert.That(book.Category, Is.SameAs(category));
            Assert.That(book.BorrowingDetails, Is.SameAs(borrowingDetails));
            Assert.That(book.BorrowingDetails.Count, Is.EqualTo(2));
        }
    }
} 