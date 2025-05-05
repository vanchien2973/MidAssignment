using System;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    public class UnitOfWorkTests : TestBase
    {
        private IUnitOfWork _unitOfWork;
        
        public override async Task Setup()
        {
            await base.Setup();
            _unitOfWork = new UnitOfWork(DbContext);
        }
        
        protected override async Task SeedDataAsync()
        {
            // Seed a test category
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Test Category",
                Description = "Test Category Description"
            };
            
            await DbContext.Categories.AddAsync(category);
            
            // Seed a test user
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
            await DbContext.SaveChangesAsync();
        }
        
        [Test]
        public void UnitOfWork_RepositoryInstances_AreNotNull()
        {
            // Assert
            Assert.That(_unitOfWork.Books, Is.Not.Null);
            Assert.That(_unitOfWork.Categories, Is.Not.Null);
            Assert.That(_unitOfWork.Users, Is.Not.Null);
            Assert.That(_unitOfWork.BookBorrowingRequests, Is.Not.Null);
            Assert.That(_unitOfWork.BookBorrowingRequestDetails, Is.Not.Null);
            Assert.That(_unitOfWork.UserActivityLogs, Is.Not.Null);
        }
        
        [Test]
        public async Task SaveChangesAsync_ReturnsNumberOfChanges()
        {
            // Arrange
            var newBook = new Book
            {
                BookId = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "123-456-789-0",
                PublishedYear = 2020,
                CategoryId = (await DbContext.Categories.FirstAsync()).CategoryId,
                TotalCopies = 10,
                AvailableCopies = 10,
                IsActive = true
            };
            
            await _unitOfWork.Books.CreateAsync(newBook);
            
            // Act
            var changes = await _unitOfWork.SaveChangesAsync();
            
            // Assert
            Assert.That(changes, Is.EqualTo(1));
            
            // Verify book was saved
            var savedBook = await DbContext.Books.FindAsync(newBook.BookId);
            Assert.That(savedBook, Is.Not.Null);
        }
        
        [Test]
        public async Task Transaction_CommitTransaction_SavesChanges()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            
            // Act
            await _unitOfWork.BeginTransactionAsync();
            
            var book = new Book
            {
                BookId = bookId,
                Title = "Transaction Test Book",
                Author = "Test Author",
                ISBN = "123-456-789-1",
                PublishedYear = 2021,
                CategoryId = (await DbContext.Categories.FirstAsync()).CategoryId,
                TotalCopies = 5,
                AvailableCopies = 5,
                IsActive = true
            };
            
            await _unitOfWork.Books.CreateAsync(book);
            await _unitOfWork.CommitTransactionAsync();
            
            // Assert - Book should be saved to the database
            var savedBook = await DbContext.Books.FindAsync(bookId);
            Assert.That(savedBook, Is.Not.Null);
            Assert.That(savedBook.Title, Is.EqualTo("Transaction Test Book"));
        }
        
        [Test]
        public async Task Transaction_RollbackTransaction_DiscardsChanges()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            
            // Act
            await _unitOfWork.BeginTransactionAsync();
            
            var book = new Book
            {
                BookId = bookId,
                Title = "Rollback Test Book",
                Author = "Test Author",
                ISBN = "123-456-789-2",
                PublishedYear = 2022,
                CategoryId = (await DbContext.Categories.FirstAsync()).CategoryId,
                TotalCopies = 3,
                AvailableCopies = 3,
                IsActive = true
            };
            
            await _unitOfWork.Books.CreateAsync(book);
            await _unitOfWork.RollbackTransactionAsync();
            
            // Assert - Book should not be saved to the database
            var savedBook = await DbContext.Books.FindAsync(bookId);
            Assert.That(savedBook, Is.Null);
        }
        
        [Test]
        public async Task UnitOfWork_MultipleOperations_InSingleTransaction()
        {
            // Arrange
            var userId = (await DbContext.Users.FirstAsync()).UserId;
            var categoryId = (await DbContext.Categories.FirstAsync()).CategoryId;
            var bookId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var detailId = Guid.NewGuid();
            
            // Act
            await _unitOfWork.BeginTransactionAsync();
            
            // Create a book
            var book = new Book
            {
                BookId = bookId,
                Title = "Complex Transaction Book",
                Author = "Test Author",
                ISBN = "123-456-789-3",
                PublishedYear = 2023,
                CategoryId = categoryId,
                TotalCopies = 2,
                AvailableCopies = 2,
                IsActive = true
            };
            
            await _unitOfWork.Books.CreateAsync(book);
            
            // Create a borrowing request
            var request = new BookBorrowingRequest
            {
                RequestId = requestId,
                UserId = userId,
                RequestDate = DateTime.UtcNow,
                Status = RequestStatus.Pending
            };
            
            await _unitOfWork.BookBorrowingRequests.CreateAsync(request);
            
            // Create a request detail
            var detail = new BookBorrowingRequestDetail
            {
                DetailId = detailId,
                RequestId = requestId,
                BookId = bookId,
                Status = BookStatus.Borrowed,
                BorrowedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14)
            };
            
            await _unitOfWork.BookBorrowingRequestDetails.CreateAsync(detail);
            
            await _unitOfWork.CommitTransactionAsync();
            
            // Assert - All entities should be saved
            var savedBook = await DbContext.Books.FindAsync(bookId);
            var savedRequest = await DbContext.BookBorrowingRequests.FindAsync(requestId);
            var savedDetail = await DbContext.BookBorrowingRequestDetails.FindAsync(detailId);
            
            Assert.That(savedBook, Is.Not.Null);
            Assert.That(savedRequest, Is.Not.Null);
            Assert.That(savedDetail, Is.Not.Null);
            Assert.That(savedDetail.RequestId, Is.EqualTo(savedRequest.RequestId));
            Assert.That(savedDetail.BookId, Is.EqualTo(savedBook.BookId));
        }
    }
} 