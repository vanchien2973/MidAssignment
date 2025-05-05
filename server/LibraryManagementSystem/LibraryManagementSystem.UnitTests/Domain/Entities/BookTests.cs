using System;
using LibraryManagementSystem.Domain.Entities;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Domain.Entities
{
    [TestFixture]
    public class BookTests
    {
        [Test]
        public void Book_InitializeWithValidProperties_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var title = "Test Book";
            var author = "Test Author";
            var isbn = "1234567890";
            var publishedYear = 2023;
            var publisher = "Test Publisher";
            var description = "Test Description";
            var totalCopies = 10;
            var availableCopies = 5;
            var createdDate = DateTime.UtcNow;
            var isActive = true;

            // Act
            var book = new Book
            {
                BookId = bookId,
                Title = title,
                Author = author,
                CategoryId = categoryId,
                ISBN = isbn,
                PublishedYear = publishedYear,
                Publisher = publisher,
                Description = description,
                TotalCopies = totalCopies,
                AvailableCopies = availableCopies,
                CreatedDate = createdDate,
                IsActive = isActive
            };

            // Assert
            Assert.That(book.BookId, Is.EqualTo(bookId));
            Assert.That(book.Title, Is.EqualTo(title));
            Assert.That(book.Author, Is.EqualTo(author));
            Assert.That(book.CategoryId, Is.EqualTo(categoryId));
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
        public void Book_NavigationProperties_ShouldBeInitializedToNull()
        {
            // Arrange & Act
            var book = new Book();

            // Assert
            Assert.That(book.Category, Is.Null);
            Assert.That(book.BorrowingDetails, Is.Null);
        }

        [Test]
        public void Book_DefaultValues_ShouldBeSetCorrectly()
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
            Assert.That(book.IsActive, Is.False);
        }
    }
} 