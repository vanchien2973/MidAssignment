using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    public class BookRepositoryTests : TestBase
    {
        private BookRepository _bookRepository;
        private Guid _categoryId;

        public override async Task Setup()
        {
            await base.Setup();
            _bookRepository = new BookRepository(DbContext);
        }

        protected override async Task SeedDataAsync()
        {
            // Create a test category
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Test Category",
                Description = "Test Category Description"
            };
            _categoryId = category.CategoryId;
            
            await DbContext.Categories.AddAsync(category);

            // Add test books
            var books = new[]
            {
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Test Book 1",
                    Author = "Test Author 1",
                    ISBN = "123-456-789-1",
                    PublishedYear = 2021,
                    CategoryId = category.CategoryId,
                    TotalCopies = 10,
                    AvailableCopies = 5,
                    IsActive = true
                },
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Test Book 2",
                    Author = "Test Author 2",
                    ISBN = "123-456-789-2",
                    PublishedYear = 2022,
                    CategoryId = category.CategoryId,
                    TotalCopies = 8,
                    AvailableCopies = 0,
                    IsActive = true
                },
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Test Book 3",
                    Author = "Test Author 3",
                    ISBN = "123-456-789-3",
                    PublishedYear = 2023,
                    CategoryId = category.CategoryId,
                    TotalCopies = 5,
                    AvailableCopies = 3,
                    IsActive = false
                }
            };
            
            await DbContext.Books.AddRangeAsync(books);
            await DbContext.SaveChangesAsync();
        }

        [Test]
        public async Task GetByIdAsync_ExistingBook_ReturnsCorrectBook()
        {
            // Arrange
            var existingBook = await DbContext.Books.FirstAsync();
            
            // Act
            var book = await _bookRepository.GetByIdAsync(existingBook.BookId);
            
            // Assert
            Assert.That(book, Is.Not.Null);
            Assert.That(book.BookId, Is.EqualTo(existingBook.BookId));
            Assert.That(book.Title, Is.EqualTo(existingBook.Title));
            Assert.That(book.Category, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_NonExistingBook_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            // Act
            var book = await _bookRepository.GetByIdAsync(nonExistingId);
            
            // Assert
            Assert.That(book, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_DefaultParameters_ReturnsPaginatedResults()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            
            // Act
            var books = (await _bookRepository.GetAllAsync(pageNumber, pageSize)).ToList();
            
            // Assert
            Assert.That(books, Is.Not.Null);
            Assert.That(books.Count, Is.EqualTo(pageSize));
        }

        [Test]
        public async Task GetAllAsync_WithSorting_ReturnsSortedResults()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            
            // Act - Sort by title ascending
            var booksAsc = (await _bookRepository.GetAllAsync(pageNumber, pageSize, "title", "asc")).ToList();
            
            // Assert
            Assert.That(booksAsc, Is.Not.Null);
            Assert.That(booksAsc, Is.Ordered.By("Title"));
            
            // Act - Sort by published year descending
            var booksDesc = (await _bookRepository.GetAllAsync(pageNumber, pageSize, "year", "desc")).ToList();
            
            // Assert
            Assert.That(booksDesc, Is.Not.Null);
            Assert.That(booksDesc, Is.Ordered.Descending.By("PublishedYear"));
        }

        [Test]
        public async Task GetByCategoryIdAsync_ExistingCategory_ReturnsCorrectBooks()
        {
            // Act
            var books = (await _bookRepository.GetByCategoryIdAsync(_categoryId, 1, 10)).ToList();
            
            // Assert
            Assert.That(books, Is.Not.Null);
            Assert.That(books.Count, Is.EqualTo(3));
            Assert.That(books.All(b => b.CategoryId == _categoryId), Is.True);
        }

        [Test]
        public async Task GetByCategoryIdAsync_NonExistingCategory_ReturnsEmptyList()
        {
            // Arrange
            var nonExistingCategoryId = Guid.NewGuid();
            
            // Act
            var books = (await _bookRepository.GetByCategoryIdAsync(nonExistingCategoryId, 1, 10)).ToList();
            
            // Assert
            Assert.That(books, Is.Empty);
        }

        [Test]
        public async Task GetAvailableBooksAsync_ReturnsOnlyAvailableAndActiveBooks()
        {
            // Act
            var books = (await _bookRepository.GetAvailableBooksAsync(1, 10)).ToList();
            
            // Assert
            Assert.That(books, Is.Not.Null);
            Assert.That(books.Count, Is.EqualTo(1)); // Only Test Book 1 is both available and active
            Assert.That(books.All(b => b.AvailableCopies > 0 && b.IsActive), Is.True);
        }

        [Test]
        public async Task IsbnExistsAsync_ExistingIsbn_ReturnsTrue()
        {
            // Act
            var exists = await _bookRepository.IsbnExistsAsync("123-456-789-1");
            
            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task IsbnExistsAsync_NonExistingIsbn_ReturnsFalse()
        {
            // Act
            var exists = await _bookRepository.IsbnExistsAsync("non-existing-isbn");
            
            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task CreateAsync_ValidBook_AddsBookToContext()
        {
            // Arrange
            var newBook = new Book
            {
                BookId = Guid.NewGuid(),
                Title = "New Test Book",
                Author = "New Test Author",
                ISBN = "123-456-789-4",
                PublishedYear = 2024,
                CategoryId = _categoryId,
                TotalCopies = 15,
                AvailableCopies = 15,
                IsActive = true
            };
            
            // Act
            var result = await _bookRepository.CreateAsync(newBook);
            await DbContext.SaveChangesAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BookId, Is.EqualTo(newBook.BookId));
            
            // Verify book was added to context
            var addedBook = await DbContext.Books.FindAsync(newBook.BookId);
            Assert.That(addedBook, Is.Not.Null);
            Assert.That(addedBook.Title, Is.EqualTo("New Test Book"));
        }

        [Test]
        public async Task UpdateAsync_ExistingBook_UpdatesBookInContext()
        {
            // Arrange
            var existingBook = await DbContext.Books.FirstAsync();
            existingBook.Title = "Updated Title";
            existingBook.Author = "Updated Author";
            
            // Act
            await _bookRepository.UpdateAsync(existingBook);
            await DbContext.SaveChangesAsync();
            
            // Assert - Reload from database to verify changes
            DbContext.Entry(existingBook).Reload();
            Assert.That(existingBook.Title, Is.EqualTo("Updated Title"));
            Assert.That(existingBook.Author, Is.EqualTo("Updated Author"));
        }

        [Test]
        public async Task DeleteAsync_ExistingBook_RemovesBookFromContext()
        {
            // Arrange
            var existingBook = await DbContext.Books.FirstAsync();
            var bookId = existingBook.BookId;
            
            // Act
            await _bookRepository.DeleteAsync(bookId);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var deletedBook = await DbContext.Books.FindAsync(bookId);
            Assert.That(deletedBook, Is.Null);
        }

        [Test]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            // Act
            var count = await _bookRepository.CountAsync();
            
            // Assert
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public async Task CountByCategoryAsync_ExistingCategory_ReturnsCorrectCount()
        {
            // Act
            var count = await _bookRepository.CountByCategoryAsync(_categoryId);
            
            // Assert
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public async Task CountAvailableBooksAsync_ReturnsCorrectCount()
        {
            // Act
            var count = await _bookRepository.CountAvailableBooksAsync();
            
            // Assert
            Assert.That(count, Is.EqualTo(1)); // Only Test Book 1 is both available and active
        }

        [Test]
        public async Task HasBooksInCategoryAsync_ExistingCategoryWithBooks_ReturnsTrue()
        {
            // Act
            var result = await _bookRepository.HasBooksInCategoryAsync(_categoryId);
            
            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task HasBooksInCategoryAsync_NonExistingCategory_ReturnsFalse()
        {
            // Arrange
            var nonExistingCategoryId = Guid.NewGuid();
            
            // Act
            var result = await _bookRepository.HasBooksInCategoryAsync(nonExistingCategoryId);
            
            // Assert
            Assert.That(result, Is.False);
        }
    }
} 