using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    public class BookBorrowingRequestRepositoryTests : TestBase
    {
        private BookBorrowingRequestRepository _borrowingRequestRepository;
        private BookBorrowingRequestDetailRepository _borrowingRequestDetailRepository;
        private Guid _userId;
        private Guid _bookId;

        public override async Task Setup()
        {
            await base.Setup();
            _borrowingRequestRepository = new BookBorrowingRequestRepository(DbContext);
            _borrowingRequestDetailRepository = new BookBorrowingRequestDetailRepository(DbContext);
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
            var book = new Book
            {
                BookId = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "123-456-789-0",
                PublishedYear = 2020,
                CategoryId = category.CategoryId,
                TotalCopies = 10,
                AvailableCopies = 5,
                IsActive = true
            };
            
            _bookId = book.BookId;
            await DbContext.Books.AddAsync(book);

            // Create test users
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
            
            _userId = user.UserId;
            await DbContext.Users.AddAsync(user);

            // Create test borrowing requests with details
            var requests = new[]
            {
                new BookBorrowingRequest
                {
                    RequestId = Guid.NewGuid(),
                    UserId = user.UserId,
                    RequestDate = DateTime.UtcNow.AddDays(-10),
                    Status = RequestStatus.Approved,
                    ProcessedDate = DateTime.UtcNow.AddDays(-9),
                    ProcessedByUserId = user.UserId
                },
                new BookBorrowingRequest
                {
                    RequestId = Guid.NewGuid(),
                    UserId = user.UserId,
                    RequestDate = DateTime.UtcNow.AddDays(-5),
                    Status = RequestStatus.Pending,
                    ProcessedDate = null,
                    ProcessedByUserId = null
                },
                new BookBorrowingRequest
                {
                    RequestId = Guid.NewGuid(),
                    UserId = user.UserId,
                    RequestDate = DateTime.UtcNow.AddDays(-3),
                    Status = RequestStatus.Rejected,
                    ProcessedDate = DateTime.UtcNow.AddDays(-2),
                    ProcessedByUserId = user.UserId
                }
            };
            
            await DbContext.BookBorrowingRequests.AddRangeAsync(requests);
            await DbContext.SaveChangesAsync();

            // Add details to the first request
            var details = new BookBorrowingRequestDetail
            {
                DetailId = Guid.NewGuid(),
                RequestId = requests[0].RequestId,
                BookId = book.BookId,
                Status = BookStatus.Borrowed,
                BorrowedDate = DateTime.UtcNow.AddDays(-9),
                DueDate = DateTime.UtcNow.AddDays(21),
                ReturnedDate = null
            };
            
            await DbContext.BookBorrowingRequestDetails.AddAsync(details);
            await DbContext.SaveChangesAsync();
        }

        [Test]
        public async Task GetByIdAsync_ExistingRequest_ReturnsCorrectRequest()
        {
            // Arrange
            var existingRequest = await DbContext.BookBorrowingRequests.FirstAsync();
            
            // Act
            var request = await _borrowingRequestRepository.GetByIdAsync(existingRequest.RequestId);
            
            // Assert
            Assert.That(request, Is.Not.Null);
            Assert.That(request.RequestId, Is.EqualTo(existingRequest.RequestId));
            Assert.That(request.User, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_NonExistingRequest_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            // Act
            var request = await _borrowingRequestRepository.GetByIdAsync(nonExistingId);
            
            // Assert
            Assert.That(request, Is.Null);
        }

        [Test]
        public async Task GetByIdWithDetailsAsync_ExistingRequestWithDetails_ReturnsRequestWithDetails()
        {
            // Arrange
            var requestWithDetails = await DbContext.BookBorrowingRequests
                .Where(r => r.Status == RequestStatus.Approved)
                .FirstAsync();
            
            // Act
            var result = await _borrowingRequestRepository.GetByIdWithDetailsAsync(requestWithDetails.RequestId);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.RequestId, Is.EqualTo(requestWithDetails.RequestId));
            Assert.That(result.Details, Is.Not.Null);
            Assert.That(result.Details.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllAsync_DefaultParameters_ReturnsPaginatedResults()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            
            // Act
            var requests = (await _borrowingRequestRepository.GetAllAsync(pageNumber, pageSize)).ToList();
            
            // Assert
            Assert.That(requests, Is.Not.Null);
            Assert.That(requests.Count, Is.EqualTo(pageSize));
        }

        [Test]
        public async Task GetByUserIdAsync_ExistingUser_ReturnsUserRequests()
        {
            // Act
            var requests = (await _borrowingRequestRepository.GetByUserIdAsync(_userId, 1, 10)).ToList();
            
            // Assert
            Assert.That(requests, Is.Not.Null);
            Assert.That(requests.Count, Is.EqualTo(3)); // All requests belong to the same user
            Assert.That(requests.All(r => r.UserId == _userId), Is.True);
        }

        [Test]
        public async Task GetByStatusAsync_ExistingStatus_ReturnsRequestsWithStatus()
        {
            // Act
            var pendingRequests = (await _borrowingRequestRepository.GetByStatusAsync(RequestStatus.Pending, 1, 10)).ToList();
            
            // Assert
            Assert.That(pendingRequests, Is.Not.Null);
            Assert.That(pendingRequests.Count, Is.EqualTo(1));
            Assert.That(pendingRequests.All(r => r.Status == RequestStatus.Pending), Is.True);
        }

        [Test]
        public async Task CreateAsync_ValidRequest_AddsRequestToContext()
        {
            // Arrange
            var newRequest = new BookBorrowingRequest
            {
                RequestId = Guid.NewGuid(),
                UserId = _userId,
                RequestDate = DateTime.UtcNow,
                Status = RequestStatus.Pending,
                ProcessedDate = null,
                ProcessedByUserId = null
            };
            
            // Act
            var result = await _borrowingRequestRepository.CreateAsync(newRequest);
            await DbContext.SaveChangesAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.RequestId, Is.EqualTo(newRequest.RequestId));
            
            // Verify request was added to context
            var addedRequest = await DbContext.BookBorrowingRequests.FindAsync(newRequest.RequestId);
            Assert.That(addedRequest, Is.Not.Null);
            Assert.That(addedRequest.Status, Is.EqualTo(RequestStatus.Pending));
        }

        [Test]
        public async Task UpdateAsync_ExistingRequest_UpdatesRequestInContext()
        {
            // Arrange
            var existingRequest = await DbContext.BookBorrowingRequests
                .Where(r => r.Status == RequestStatus.Pending)
                .FirstAsync();
            
            existingRequest.Status = RequestStatus.Approved;
            existingRequest.ProcessedDate = DateTime.UtcNow;
            existingRequest.ProcessedByUserId = _userId;
            
            // Act
            await _borrowingRequestRepository.UpdateAsync(existingRequest);
            await DbContext.SaveChangesAsync();
            
            // Assert - Reload from database to verify changes
            DbContext.Entry(existingRequest).Reload();
            Assert.That(existingRequest.Status, Is.EqualTo(RequestStatus.Approved));
            Assert.That(existingRequest.ProcessedDate, Is.Not.Null);
            Assert.That(existingRequest.ProcessedByUserId, Is.EqualTo(_userId));
        }

        [Test]
        public async Task GetOverdueRequestsAsync_ReturnsOverdueRequests()
        {
            // Arrange - Create an overdue request
            var approvedRequest = await DbContext.BookBorrowingRequests
                .Where(r => r.Status == RequestStatus.Approved)
                .FirstAsync();
            
            var detail = await DbContext.BookBorrowingRequestDetails
                .Where(d => d.RequestId == approvedRequest.RequestId)
                .FirstAsync();
            
            // Set the due date to the past
            detail.DueDate = DateTime.UtcNow.AddDays(-1);
            await DbContext.SaveChangesAsync();
            
            // Act
            var overdueRequests = (await _borrowingRequestRepository.GetOverdueRequestsAsync(1, 10)).ToList();
            
            // Assert
            Assert.That(overdueRequests, Is.Not.Null);
            Assert.That(overdueRequests.Count, Is.GreaterThan(0));
            Assert.That(overdueRequests.Any(r => r.RequestId == approvedRequest.RequestId), Is.True);
        }

        [Test]
        public async Task GetBorrowingHistoryAsync_ReturnsRequestsWithCompletedDetails()
        {
            // Arrange - Create a completed request (book returned)
            var approvedRequest = await DbContext.BookBorrowingRequests
                .Where(r => r.Status == RequestStatus.Approved)
                .FirstAsync();
            
            var detail = await DbContext.BookBorrowingRequestDetails
                .Where(d => d.RequestId == approvedRequest.RequestId)
                .FirstAsync();
            
            // Set the returned date to mark it as completed
            detail.ReturnedDate = DateTime.UtcNow;
            detail.Status = BookStatus.Returned;
            await DbContext.SaveChangesAsync();
            
            // Act
            var history = (await _borrowingRequestRepository.GetBorrowingHistoryAsync(_userId, 1, 10)).ToList();
            
            // Assert
            Assert.That(history, Is.Not.Null);
            Assert.That(history.Count, Is.GreaterThan(0));
            Assert.That(history.Any(r => r.RequestId == approvedRequest.RequestId), Is.True);
        }

        [Test]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            // Act
            var count = await _borrowingRequestRepository.CountAsync();
            
            // Assert
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public async Task CountByUserIdAsync_ExistingUser_ReturnsCorrectCount()
        {
            // Act
            var count = await _borrowingRequestRepository.CountByUserIdAsync(_userId);
            
            // Assert
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public async Task CountByStatusAsync_ExistingStatus_ReturnsCorrectCount()
        {
            // Act
            var count = await _borrowingRequestRepository.CountByStatusAsync(RequestStatus.Pending);
            
            // Assert
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task HasActiveRequestsForBookAsync_BookWithActiveRequests_ReturnsTrue()
        {
            // Act
            var result = await _borrowingRequestRepository.HasActiveRequestsForBookAsync(_bookId);
            
            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task HasActiveRequestsForBookAsync_BookWithNoActiveRequests_ReturnsFalse()
        {
            // Arrange - Create a book with no active requests
            var newBook = new Book
            {
                BookId = Guid.NewGuid(),
                Title = "New Book Without Requests",
                Author = "Test Author",
                ISBN = "123-456-789-X",
                PublishedYear = 2022,
                CategoryId = (await DbContext.Categories.FirstAsync()).CategoryId,
                TotalCopies = 5,
                AvailableCopies = 5,
                IsActive = true
            };
            
            await DbContext.Books.AddAsync(newBook);
            await DbContext.SaveChangesAsync();
            
            // Act
            var result = await _borrowingRequestRepository.HasActiveRequestsForBookAsync(newBook.BookId);
            
            // Assert
            Assert.That(result, Is.False);
        }
    }
} 