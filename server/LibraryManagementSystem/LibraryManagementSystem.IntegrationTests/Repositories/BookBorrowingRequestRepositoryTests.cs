using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Context;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    [TestFixture]
    public class BookBorrowingRequestRepositoryTests
    {
        private DbContextOptions<LibraryDbContext> _options;
        private LibraryDbContext _context;
        private BookBorrowingRequestRepository _repository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new LibraryDbContext(_options);
            _repository = new BookBorrowingRequestRepository(_context);

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

            var user1 = new User
            {
                UserId = 1,
                Username = "user1",
                FullName = "User One",
                Email = "user1@example.com",
                Password = "hashed_password",
                IsActive = true
            };

            var user2 = new User
            {
                UserId = 2,
                Username = "user2",
                FullName = "User Two",
                Email = "user2@example.com",
                Password = "hashed_password",
                IsActive = true
            };
            _context.Users.AddRange(user1, user2);

            // Tạo các đơn mượn sách với các trạng thái khác nhau
            var request1 = new BookBorrowingRequest
            {
                RequestId = Guid.NewGuid(),
                RequestorId = user1.UserId,
                RequestDate = DateTime.UtcNow.AddDays(-10),
                Status = BorrowingRequestStatus.Approved,
                ApproverId = user2.UserId,
                ApprovalDate = DateTime.UtcNow.AddDays(-9),
                Notes = "Approved request"
            };

            var request2 = new BookBorrowingRequest
            {
                RequestId = Guid.NewGuid(),
                RequestorId = user1.UserId,
                RequestDate = DateTime.UtcNow.AddDays(-5),
                Status = BorrowingRequestStatus.Waiting,
                Notes = "Waiting for approval"
            };

            var request3 = new BookBorrowingRequest
            {
                RequestId = Guid.NewGuid(),
                RequestorId = user2.UserId,
                RequestDate = DateTime.UtcNow.AddDays(-3),
                Status = BorrowingRequestStatus.Rejected,
                ApproverId = user1.UserId,
                ApprovalDate = DateTime.UtcNow.AddDays(-2),
                Notes = "Rejected request"
            };
            _context.BookBorrowingRequests.AddRange(request1, request2, request3);

            // Tạo chi tiết đơn mượn
            var detail1 = new BookBorrowingRequestDetail
            {
                DetailId = Guid.NewGuid(),
                RequestId = request1.RequestId,
                BookId = book1.BookId,
                Status = BorrowingDetailStatus.Borrowing,
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            var detail2 = new BookBorrowingRequestDetail
            {
                DetailId = Guid.NewGuid(),
                RequestId = request1.RequestId,
                BookId = book2.BookId,
                Status = BorrowingDetailStatus.Returned,
                DueDate = DateTime.UtcNow.AddDays(7),
                ReturnDate = DateTime.UtcNow
            };

            var detail3 = new BookBorrowingRequestDetail
            {
                DetailId = Guid.NewGuid(),
                RequestId = request2.RequestId,
                BookId = book1.BookId,
                Status = BorrowingDetailStatus.Borrowing,
                DueDate = null
            };
            _context.BookBorrowingRequestDetails.AddRange(detail1, detail2, detail3);

            _context.SaveChanges();
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsRequestWithDetails()
        {
            // Arrange
            var requestId = _context.BookBorrowingRequests
                .Where(r => r.Status == BorrowingRequestStatus.Approved)
                .First().RequestId;

            // Act
            var result = await _repository.GetByIdAsync(requestId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.RequestId, Is.EqualTo(requestId));
            Assert.That(result.Requestor, Is.Not.Null);
            Assert.That(result.Approver, Is.Not.Null);
            Assert.That(result.RequestDetails, Is.Not.Null);
            Assert.That(result.RequestDetails.Count, Is.EqualTo(2));
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
        public async Task GetByUserIdAsync_ExistingUserId_ReturnsRequests()
        {
            // Arrange
            int userId = 1;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetByUserIdAsync(userId, pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            foreach (var request in result)
            {
                Assert.That(request.RequestorId, Is.EqualTo(userId));
                Assert.That(request.Requestor, Is.Not.Null);
                Assert.That(request.RequestDetails, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetPendingRequestsAsync_ReturnsPendingRequests()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetPendingRequestsAsync(pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Status, Is.EqualTo(BorrowingRequestStatus.Waiting));
        }

        [Test]
        public async Task GetByStatusAsync_ReturnsRequestsWithSpecifiedStatus()
        {
            // Arrange
            var status = BorrowingRequestStatus.Approved;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetByStatusAsync(status, pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Status, Is.EqualTo(status));
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllRequests()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetAllAsync(pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task CreateAsync_ValidRequest_AddsToDatabase()
        {
            // Arrange
            var userId = _context.Users.First().UserId;
            var newRequest = new BookBorrowingRequest
            {
                RequestId = Guid.NewGuid(),
                RequestorId = userId,
                RequestDate = DateTime.UtcNow,
                Status = BorrowingRequestStatus.Waiting,
                Notes = "New test request"
            };

            // Act
            var result = await _repository.CreateAsync(newRequest);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            var savedRequest = await _context.BookBorrowingRequests.FindAsync(newRequest.RequestId);
            Assert.That(savedRequest, Is.Not.Null);
            Assert.That(savedRequest.RequestorId, Is.EqualTo(userId));
            Assert.That(savedRequest.Status, Is.EqualTo(BorrowingRequestStatus.Waiting));
        }

        [Test]
        public async Task UpdateAsync_ExistingRequest_UpdatesInDatabase()
        {
            // Arrange
            var request = await _context.BookBorrowingRequests
                .Where(r => r.Status == BorrowingRequestStatus.Waiting)
                .FirstAsync();
            request.Status = BorrowingRequestStatus.Approved;
            request.ApproverId = 2;
            request.ApprovalDate = DateTime.UtcNow;

            // Act
            await _repository.UpdateAsync(request);
            await _context.SaveChangesAsync();

            // Assert
            var updatedRequest = await _context.BookBorrowingRequests.FindAsync(request.RequestId);
            Assert.That(updatedRequest, Is.Not.Null);
            Assert.That(updatedRequest.Status, Is.EqualTo(BorrowingRequestStatus.Approved));
            Assert.That(updatedRequest.ApproverId, Is.EqualTo(2));
            Assert.That(updatedRequest.ApprovalDate, Is.Not.Null);
        }

        [Test]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public async Task HasUserActiveBookLoansAsync_UserWithActiveLoans_ReturnsTrue()
        {
            // Arrange
            int userIdWithActiveLoans = 1;

            // Act
            var result = await _repository.HasUserActiveBookLoansAsync(userIdWithActiveLoans);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
