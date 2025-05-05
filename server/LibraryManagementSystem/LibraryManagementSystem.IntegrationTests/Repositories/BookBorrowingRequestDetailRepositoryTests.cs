using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    public class BookBorrowingRequestDetailRepositoryTests : TestBase
    {
        private BookBorrowingRequestDetailRepository _requestDetailRepository;
        private Guid _requestId;
        private Guid _bookId;

        public override async Task Setup()
        {
            await base.Setup();
            _requestDetailRepository = new BookBorrowingRequestDetailRepository(DbContext);
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
            
            await DbContext.Categories.AddAsync(category);

            // Create test books
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
                    AvailableCopies = 4,
                    IsActive = true
                }
            };
            
            _bookId = books[0].BookId;
            await DbContext.Books.AddRangeAsync(books);

            // Create test user
            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserName = "testuser",
                PasswordHash = "hashed_password",
                Email = "test@example.com",
                FullName = "Test User",
                Role = UserRole.Member,
                IsActive = true
            };
            
            await DbContext.Users.AddAsync(user);

            // Create test borrowing request
            var request = new BookBorrowingRequest
            {
                RequestId = Guid.NewGuid(),
                UserId = user.UserId,
                RequestDate = DateTime.UtcNow.AddDays(-10),
                Status = RequestStatus.Approved,
                ProcessedDate = DateTime.UtcNow.AddDays(-9),
                ProcessedByUserId = user.UserId
            };
            
            _requestId = request.RequestId;
            await DbContext.BookBorrowingRequests.AddAsync(request);
            await DbContext.SaveChangesAsync();

            // Add details to the request
            var details = new[]
            {
                new BookBorrowingRequestDetail
                {
                    DetailId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    BookId = books[0].BookId,
                    Status = BookStatus.Borrowed,
                    BorrowedDate = DateTime.UtcNow.AddDays(-9),
                    DueDate = DateTime.UtcNow.AddDays(21),
                    ReturnedDate = null
                },
                new BookBorrowingRequestDetail
                {
                    DetailId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    BookId = books[1].BookId,
                    Status = BookStatus.Returned,
                    BorrowedDate = DateTime.UtcNow.AddDays(-9),
                    DueDate = DateTime.UtcNow.AddDays(21),
                    ReturnedDate = DateTime.UtcNow.AddDays(-2)
                }
            };
            
            await DbContext.BookBorrowingRequestDetails.AddRangeAsync(details);
            await DbContext.SaveChangesAsync();
        }

        [Test]
        public async Task GetByIdAsync_ExistingDetail_ReturnsCorrectDetail()
        {
            // Arrange
            var existingDetail = await DbContext.BookBorrowingRequestDetails.FirstAsync();
            
            // Act
            var detail = await _requestDetailRepository.GetByIdAsync(existingDetail.DetailId);
            
            // Assert
            Assert.That(detail, Is.Not.Null);
            Assert.That(detail.DetailId, Is.EqualTo(existingDetail.DetailId));
            Assert.That(detail.Book, Is.Not.Null);
            Assert.That(detail.Request, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_NonExistingDetail_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            // Act
            var detail = await _requestDetailRepository.GetByIdAsync(nonExistingId);
            
            // Assert
            Assert.That(detail, Is.Null);
        }

        [Test]
        public async Task GetDetailsByRequestIdAsync_ExistingRequest_ReturnsAllDetails()
        {
            // Act
            var details = (await _requestDetailRepository.GetDetailsByRequestIdAsync(_requestId)).ToList();
            
            // Assert
            Assert.That(details, Is.Not.Null);
            Assert.That(details.Count, Is.EqualTo(2));
            Assert.That(details.All(d => d.RequestId == _requestId), Is.True);
        }

        [Test]
        public async Task GetDetailsByRequestIdAsync_NonExistingRequest_ReturnsEmptyList()
        {
            // Arrange
            var nonExistingRequestId = Guid.NewGuid();
            
            // Act
            var details = (await _requestDetailRepository.GetDetailsByRequestIdAsync(nonExistingRequestId)).ToList();
            
            // Assert
            Assert.That(details, Is.Empty);
        }

        [Test]
        public async Task GetActiveDetailsByBookIdAsync_BookWithActiveBorrowings_ReturnsDetails()
        {
            // Act
            var details = (await _requestDetailRepository.GetActiveDetailsByBookIdAsync(_bookId)).ToList();
            
            // Assert
            Assert.That(details, Is.Not.Null);
            Assert.That(details.Count, Is.EqualTo(1)); // Only one active borrowing for this book
            Assert.That(details.All(d => d.BookId == _bookId && d.Status == BookStatus.Borrowed), Is.True);
        }

        [Test]
        public async Task GetOverdueDetailsAsync_ReturnsOverdueDetails()
        {
            // Arrange - Create an overdue detail
            var borrowedDetail = await DbContext.BookBorrowingRequestDetails
                .Where(d => d.Status == BookStatus.Borrowed)
                .FirstAsync();
            
            // Set the due date to the past
            borrowedDetail.DueDate = DateTime.UtcNow.AddDays(-1);
            await DbContext.SaveChangesAsync();
            
            // Act
            var overdueDetails = (await _requestDetailRepository.GetOverdueDetailsAsync()).ToList();
            
            // Assert
            Assert.That(overdueDetails, Is.Not.Null);
            Assert.That(overdueDetails.Count, Is.GreaterThan(0));
            Assert.That(overdueDetails.Any(d => d.DetailId == borrowedDetail.DetailId), Is.True);
        }

        [Test]
        public async Task CreateAsync_ValidDetail_AddsDetailToContext()
        {
            // Arrange
            var newBook = new Book
            {
                BookId = Guid.NewGuid(),
                Title = "New Test Book",
                Author = "New Author",
                ISBN = "123-456-789-3",
                PublishedYear = 2023,
                CategoryId = (await DbContext.Categories.FirstAsync()).CategoryId,
                TotalCopies = 5,
                AvailableCopies = 5,
                IsActive = true
            };
            
            await DbContext.Books.AddAsync(newBook);
            await DbContext.SaveChangesAsync();
            
            var newDetail = new BookBorrowingRequestDetail
            {
                DetailId = Guid.NewGuid(),
                RequestId = _requestId,
                BookId = newBook.BookId,
                Status = BookStatus.Borrowed,
                BorrowedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(21),
                ReturnedDate = null
            };
            
            // Act
            var result = await _requestDetailRepository.CreateAsync(newDetail);
            await DbContext.SaveChangesAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.DetailId, Is.EqualTo(newDetail.DetailId));
            
            // Verify detail was added to context
            var addedDetail = await DbContext.BookBorrowingRequestDetails.FindAsync(newDetail.DetailId);
            Assert.That(addedDetail, Is.Not.Null);
            Assert.That(addedDetail.Status, Is.EqualTo(BookStatus.Borrowed));
        }

        [Test]
        public async Task UpdateAsync_ExistingDetail_UpdatesDetailInContext()
        {
            // Arrange
            var existingDetail = await DbContext.BookBorrowingRequestDetails
                .Where(d => d.Status == BookStatus.Borrowed)
                .FirstAsync();
            
            // Update to returned status
            existingDetail.Status = BookStatus.Returned;
            existingDetail.ReturnedDate = DateTime.UtcNow;
            
            // Act
            await _requestDetailRepository.UpdateAsync(existingDetail);
            await DbContext.SaveChangesAsync();
            
            // Assert - Reload from database to verify changes
            DbContext.Entry(existingDetail).Reload();
            Assert.That(existingDetail.Status, Is.EqualTo(BookStatus.Returned));
            Assert.That(existingDetail.ReturnedDate, Is.Not.Null);
        }

        [Test]
        public async Task DeleteAsync_ExistingDetail_RemovesDetailFromContext()
        {
            // Arrange
            var existingDetail = await DbContext.BookBorrowingRequestDetails.FirstAsync();
            var detailId = existingDetail.DetailId;
            
            // Act
            await _requestDetailRepository.DeleteAsync(detailId);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var deletedDetail = await DbContext.BookBorrowingRequestDetails.FindAsync(detailId);
            Assert.That(deletedDetail, Is.Null);
        }

        [Test]
        public async Task IsBookBorrowed_BorrowedBook_ReturnsTrue()
        {
            // Arrange
            var borrowedBookId = (await DbContext.BookBorrowingRequestDetails
                .Where(d => d.Status == BookStatus.Borrowed)
                .FirstAsync()).BookId;
            
            // Act
            var result = await _requestDetailRepository.IsBookBorrowed(borrowedBookId);
            
            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsBookBorrowed_ReturnedBook_ReturnsFalse()
        {
            // Arrange - Make sure there are no active borrowings for this book
            var returnedBookDetail = await DbContext.BookBorrowingRequestDetails
                .Where(d => d.Status == BookStatus.Returned)
                .FirstAsync();
            
            // Ensure there are no other active borrowings for this book
            var otherDetails = await DbContext.BookBorrowingRequestDetails
                .Where(d => d.BookId == returnedBookDetail.BookId && d.Status == BookStatus.Borrowed)
                .ToListAsync();
            
            foreach (var detail in otherDetails)
            {
                detail.Status = BookStatus.Returned;
                detail.ReturnedDate = DateTime.UtcNow;
            }
            
            await DbContext.SaveChangesAsync();
            
            // Act
            var result = await _requestDetailRepository.IsBookBorrowed(returnedBookDetail.BookId);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CountBorrowedBooksByUserAsync_UserWithBorrowedBooks_ReturnsCorrectCount()
        {
            // Arrange
            var userId = (await DbContext.BookBorrowingRequests.FirstAsync()).UserId;
            
            // Act
            var count = await _requestDetailRepository.CountBorrowedBooksByUserAsync(userId);
            
            // Assert
            Assert.That(count, Is.EqualTo(1)); // One book is borrowed, one is returned
        }
    }
} 