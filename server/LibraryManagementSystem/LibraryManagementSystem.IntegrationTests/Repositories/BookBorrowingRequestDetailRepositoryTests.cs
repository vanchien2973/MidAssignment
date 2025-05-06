using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Context;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    [TestFixture]
    public class BookBorrowingRequestDetailRepositoryTests
    {
        private DbContextOptions<LibraryDbContext> _options;
        private LibraryDbContext _context;
        private BookBorrowingRequestDetailRepository _repository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new LibraryDbContext(_options);
            _repository = new BookBorrowingRequestDetailRepository(_context);

            SeedDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            if (_repository is IDisposable disposableRepository)
            {
                disposableRepository.Dispose();
            }
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            // Tạo dữ liệu mẫu
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Fiction",
                Description = "Fiction books"
            };
            _context.Categories.Add(category);

            var book1 = new Book
            {
                BookId = Guid.NewGuid(),
                Title = "Book 1",
                Author = "Author 1",
                CategoryId = category.CategoryId,
                ISBN = "123456789",
                PublishedYear = 2020,
                TotalCopies = 5,
                AvailableCopies = 3,
                Publisher = "Publisher 1",
                Description = "Description of Book 1",
                IsActive = true
            };

            var book2 = new Book
            {
                BookId = Guid.NewGuid(),
                Title = "Book 2",
                Author = "Author 2",
                CategoryId = category.CategoryId,
                ISBN = "987654321",
                PublishedYear = 2021,
                TotalCopies = 3,
                AvailableCopies = 1,
                Publisher = "Publisher 2",
                Description = "Description of Book 2",
                IsActive = true
            };
            _context.Books.AddRange(book1, book2);

            var user = new User
            {
                UserId = 1,
                Username = "testuser",
                FullName = "Test User",
                Email = "test@example.com",
                Password = "hashed_password",
                IsActive = true
            };
            _context.Users.Add(user);

            var borrowingRequest = new BookBorrowingRequest
            {
                RequestId = Guid.NewGuid(),
                RequestorId = user.UserId,
                RequestDate = DateTime.UtcNow.AddDays(-10),
                Status = BorrowingRequestStatus.Approved,
                Notes = "Test borrowing"
            };
            _context.BookBorrowingRequests.Add(borrowingRequest);

            var dueDate = DateTime.UtcNow.AddDays(-2); // Đã quá hạn
            var detail1 = new BookBorrowingRequestDetail
            {
                DetailId = Guid.NewGuid(),
                RequestId = borrowingRequest.RequestId,
                BookId = book1.BookId,
                Status = BorrowingDetailStatus.Borrowing,
                DueDate = dueDate
            };

            var detail2 = new BookBorrowingRequestDetail
            {
                DetailId = Guid.NewGuid(),
                RequestId = borrowingRequest.RequestId,
                BookId = book2.BookId,
                Status = BorrowingDetailStatus.Returned,
                DueDate = DateTime.UtcNow.AddDays(5),
                ReturnDate = DateTime.UtcNow.AddDays(-1)
            };
            _context.BookBorrowingRequestDetails.AddRange(detail1, detail2);

            _context.SaveChanges();
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsDetail()
        {
            // Arrange
            var detailId = _context.BookBorrowingRequestDetails.First().DetailId;

            // Act
            var result = await _repository.GetByIdAsync(detailId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.DetailId, Is.EqualTo(detailId));
            Assert.That(result.Book, Is.Not.Null);
            Assert.That(result.Request, Is.Not.Null);
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
        public async Task CreateAsync_ValidDetail_AddsToDatabase()
        {
            // Arrange
            var bookId = _context.Books.First().BookId;
            var requestId = _context.BookBorrowingRequests.First().RequestId;
            var newDetail = new BookBorrowingRequestDetail
            {
                DetailId = Guid.NewGuid(),
                RequestId = requestId,
                BookId = bookId,
                Status = BorrowingDetailStatus.Borrowing,
                DueDate = DateTime.UtcNow.AddDays(14)
            };

            // Act
            var result = await _repository.CreateAsync(newDetail);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            var savedDetail = await _context.BookBorrowingRequestDetails.FindAsync(newDetail.DetailId);
            Assert.That(savedDetail, Is.Not.Null);
            Assert.That(savedDetail.BookId, Is.EqualTo(bookId));
            Assert.That(savedDetail.Status, Is.EqualTo(BorrowingDetailStatus.Borrowing));
        }

        [Test]
        public async Task UpdateAsync_ExistingDetail_UpdatesInDatabase()
        {
            // Arrange
            var detail = await _context.BookBorrowingRequestDetails
                .Where(d => d.Status == BorrowingDetailStatus.Borrowing)
                .FirstAsync();
            detail.Status = BorrowingDetailStatus.Returned;
            detail.ReturnDate = DateTime.UtcNow;

            // Act
            await _repository.UpdateAsync(detail);
            await _context.SaveChangesAsync();

            // Assert
            var updatedDetail = await _context.BookBorrowingRequestDetails.FindAsync(detail.DetailId);
            Assert.That(updatedDetail, Is.Not.Null);
            Assert.That(updatedDetail.Status, Is.EqualTo(BorrowingDetailStatus.Returned));
            Assert.That(updatedDetail.ReturnDate, Is.Not.Null);
        }

        [Test]
        public async Task GetOverdueItemsAsync_ReturnsOverdueItems()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetOverdueItemsAsync(pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Status, Is.EqualTo(BorrowingDetailStatus.Borrowing));
            Assert.That(result.First().DueDate, Is.LessThan(DateTime.Today));
        }

        [Test]
        public async Task HasActiveBorrowingsForBookAsync_WithActiveBorrowings_ReturnsTrue()
        {
            // Arrange
            var bookWithActiveBorrowing = await _context.BookBorrowingRequestDetails
                .Where(d => d.Status == BorrowingDetailStatus.Borrowing)
                .Select(d => d.BookId)
                .FirstAsync();

            // Act
            var result = await _repository.HasActiveBorrowingsForBookAsync(bookWithActiveBorrowing);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task HasActiveBorrowingsForBookAsync_WithoutActiveBorrowings_ReturnsFalse()
        {
            // Arrange
            var bookWithoutActiveBorrowing = await _context.BookBorrowingRequestDetails
                .Where(d => d.Status == BorrowingDetailStatus.Returned)
                .Select(d => d.BookId)
                .FirstAsync();

            // Act
            var result = await _repository.HasActiveBorrowingsForBookAsync(bookWithoutActiveBorrowing);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
