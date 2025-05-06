using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Data.Context;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    [TestFixture]
    public class BookRepositoryTests
    {
        private DbContextOptions<LibraryDbContext> _options;
        private LibraryDbContext _context;
        private BookRepository _repository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new LibraryDbContext(_options);
            _repository = new BookRepository(_context);

            SeedDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            if (_repository is IDisposable disposableRepository)
            {
                disposableRepository.Dispose();
            }
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }

        private void SeedDatabase()
        {
            var category1 = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Fiction",
                Description = "Fiction books",
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            };

            var category2 = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Science",
                Description = "Science books",
                CreatedDate = DateTime.UtcNow.AddDays(-25)
            };
            _context.Categories.AddRange(category1, category2);

            var books = new[]
            {
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Book 1",
                    Author = "Author 1",
                    CategoryId = category1.CategoryId,
                    ISBN = "123456789",
                    PublishedYear = 2020,
                    TotalCopies = 5,
                    AvailableCopies = 3,
                    IsActive = true,
                    Publisher = "Publisher 1",
                    Description = "Description of Book 1"
                },
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Book 2",
                    Author = "Author 2",
                    CategoryId = category1.CategoryId,
                    ISBN = "223456789",
                    PublishedYear = 2019,
                    TotalCopies = 3,
                    AvailableCopies = 0, // Không có sẵn
                    IsActive = true,
                    Publisher = "Publisher 2",
                    Description = "Description of Book 2"
                },
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Science Book",
                    Author = "Author 3",
                    CategoryId = category2.CategoryId,
                    ISBN = "323456789",
                    PublishedYear = 2021,
                    TotalCopies = 2,
                    AvailableCopies = 2,
                    IsActive = true,
                    Publisher = "Publisher 3",
                    Description = "Description of Science Book"
                },
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Inactive Book",
                    Author = "Author 4",
                    CategoryId = category2.CategoryId,
                    ISBN = "423456789",
                    PublishedYear = 2018,
                    TotalCopies = 1,
                    AvailableCopies = 1,
                    IsActive = false, // Không hoạt động
                    Publisher = "Publisher 4",
                    Description = "Description of Inactive Book"
                }
            };
            _context.Books.AddRange(books);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsBook()
        {
            // Arrange
            var bookId = _context.Books.First().BookId;

            // Act
            var result = await _repository.GetByIdAsync(bookId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BookId, Is.EqualTo(bookId));
            Assert.That(result.Category, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllBooks()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetAllAsync(pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(4));
            foreach (var book in result)
            {
                Assert.That(book.Category, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetAllAsync_WithSorting_ReturnsSortedBooks()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            string sortBy = "title";
            string sortOrder = "desc";

            // Act
            var result = await _repository.GetAllAsync(pageNumber, pageSize, sortBy, sortOrder);
            var list = result.ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(4));
            // Kiểm tra thứ tự sắp xếp giảm dần theo tiêu đề
            for (int i = 0; i < list.Count - 1; i++)
            {
                Assert.That(string.Compare(list[i].Title, list[i + 1].Title), Is.GreaterThanOrEqualTo(0));
            }
        }

        [Test]
        public async Task GetByCategoryIdAsync_ReturnsBooksInCategory()
        {
            // Arrange
            var categoryId = _context.Categories
                .Where(c => c.CategoryName == "Fiction")
                .First().CategoryId;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetByCategoryIdAsync(categoryId, pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            foreach (var book in result)
            {
                Assert.That(book.CategoryId, Is.EqualTo(categoryId));
                Assert.That(book.Category, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetAvailableBooksAsync_ReturnsOnlyAvailableAndActiveBooks()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetAvailableBooksAsync(pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2)); // Chỉ có 2 cuốn sách có sẵn và active
            foreach (var book in result)
            {
                Assert.That(book.AvailableCopies, Is.GreaterThan(0));
                Assert.That(book.IsActive, Is.True);
            }
        }

        [Test]
        public async Task IsbnExistsAsync_WithExistingIsbn_ReturnsTrue()
        {
            // Arrange
            var existingIsbn = "123456789";

            // Act
            var result = await _repository.IsbnExistsAsync(existingIsbn);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsbnExistsAsync_WithNonExistentIsbn_ReturnsFalse()
        {
            // Arrange
            var nonExistentIsbn = "999999999";

            // Act
            var result = await _repository.IsbnExistsAsync(nonExistentIsbn);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CreateAsync_ValidBook_AddsToDatabase()
        {
            // Arrange
            var categoryId = _context.Categories.First().CategoryId;
            var newBook = new Book
            {
                BookId = Guid.NewGuid(),
                Title = "New Book",
                Author = "New Author",
                CategoryId = categoryId,
                ISBN = "987654321",
                PublishedYear = 2022,
                TotalCopies = 1,
                AvailableCopies = 1,
                IsActive = true,
                Publisher = "New Publisher",
                Description = "Description of New Book"
            };

            // Act
            var result = await _repository.CreateAsync(newBook);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            var savedBook = await _context.Books.FindAsync(newBook.BookId);
            Assert.That(savedBook, Is.Not.Null);
            Assert.That(savedBook.Title, Is.EqualTo("New Book"));
        }

        [Test]
        public async Task UpdateAsync_ExistingBook_UpdatesInDatabase()
        {
            // Arrange
            var book = await _context.Books.FirstAsync();
            book.Title = "Updated Title";
            book.AvailableCopies = 2;

            // Act
            await _repository.UpdateAsync(book);
            await _context.SaveChangesAsync();

            // Assert
            var updatedBook = await _context.Books.FindAsync(book.BookId);
            Assert.That(updatedBook, Is.Not.Null);
            Assert.That(updatedBook.Title, Is.EqualTo("Updated Title"));
            Assert.That(updatedBook.AvailableCopies, Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteAsync_ExistingBook_RemovesFromDatabase()
        {
            // Arrange
            var bookId = _context.Books.First().BookId;

            // Act
            await _repository.DeleteAsync(bookId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedBook = await _context.Books.FindAsync(bookId);
            Assert.That(deletedBook, Is.Null);
        }

        [Test]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.That(result, Is.EqualTo(4));
        }

        [Test]
        public async Task CountByCategoryAsync_ReturnsCorrectCount()
        {
            // Arrange
            var categoryId = _context.Categories
                .Where(c => c.CategoryName == "Fiction")
                .First().CategoryId;

            // Act
            var result = await _repository.CountByCategoryAsync(categoryId);

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task CountAvailableBooksAsync_ReturnsCorrectCount()
        {
            // Act
            var result = await _repository.CountAvailableBooksAsync();

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task HasBooksInCategoryAsync_WithBooksInCategory_ReturnsTrue()
        {
            // Arrange
            var categoryId = _context.Categories
                .Where(c => c.CategoryName == "Fiction")
                .First().CategoryId;

            // Act
            var result = await _repository.HasBooksInCategoryAsync(categoryId);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
