using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Data;
using LibraryManagementSystem.Infrastructure.Data.Context;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    [TestFixture]
    public class UnitOfWorkTests
    {
        private DbContextOptions<LibraryDbContext> _options;
        private LibraryDbContext _context;
        private UnitOfWork _unitOfWork;
        private IServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new LibraryDbContext(_options);

            // Tạo các repository
            var services = new ServiceCollection();
            services.AddSingleton<IUserRepository>(new UserRepository(_context));
            services.AddSingleton<IBookRepository>(new BookRepository(_context));
            services.AddSingleton<ICategoryRepository>(new CategoryRepository(_context));
            services.AddSingleton<IBookBorrowingRequestRepository>(new BookBorrowingRequestRepository(_context));
            services.AddSingleton<IBookBorrowingRequestDetailRepository>(new BookBorrowingRequestDetailRepository(_context));
            services.AddSingleton<IUserActivityLogRepository>(new UserActivityLogRepository(_context));
            services.AddSingleton<LibraryDbContext>(_context);

            _serviceProvider = services.BuildServiceProvider();

            _unitOfWork = new UnitOfWork(
                _context,
                _serviceProvider.GetRequiredService<IUserRepository>(),
                _serviceProvider.GetRequiredService<IBookRepository>(),
                _serviceProvider.GetRequiredService<ICategoryRepository>(),
                _serviceProvider.GetRequiredService<IBookBorrowingRequestRepository>(),
                _serviceProvider.GetRequiredService<IBookBorrowingRequestDetailRepository>(),
                _serviceProvider.GetRequiredService<IUserActivityLogRepository>()
            );
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (_unitOfWork != null)
                {
                    _unitOfWork.Dispose();
                    _unitOfWork = null;
                }
            }
            catch { }

            try
            {
                if (_context != null)
                {
                    _context.Database.EnsureDeleted();
                    _context.Dispose();
                    _context = null;
                }
            }
            catch { }

            try
            {
                if (_serviceProvider != null)
                {
                    if (_serviceProvider is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
            catch { }
        }

        [Test]
        public void UnitOfWork_RepositoriesAreNotNull()
        {
            // Assert
            Assert.That(_unitOfWork.Users, Is.Not.Null);
            Assert.That(_unitOfWork.Books, Is.Not.Null);
            Assert.That(_unitOfWork.Categories, Is.Not.Null);
            Assert.That(_unitOfWork.BookBorrowingRequests, Is.Not.Null);
            Assert.That(_unitOfWork.BookBorrowingRequestDetails, Is.Not.Null);
            Assert.That(_unitOfWork.UserActivityLogs, Is.Not.Null);
        }

        [Test]
        public async Task SaveChangesAsync_SavesChangesToDatabase()
        {
            // Arrange
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Test Category",
                Description = "Test Description",
                CreatedDate = DateTime.UtcNow
            };
            _context.Categories.Add(category);

            // Act
            var result = await _unitOfWork.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.GreaterThan(0));
            var savedCategory = await _context.Categories.FindAsync(category.CategoryId);
            Assert.That(savedCategory, Is.Not.Null);
        }

        [Test]
        public async Task Transaction_CommitChanges_SavesChangesToDatabase()
        {
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Transaction Test Category",
                Description = "Test Description",
                CreatedDate = DateTime.UtcNow
            };
            _context.Categories.Add(category);
            
            await _unitOfWork.SaveChangesAsync();

            // Assert
            var savedCategory = await _context.Categories.FindAsync(category.CategoryId);
            Assert.That(savedCategory, Is.Not.Null);
            Assert.That(savedCategory.CategoryName, Is.EqualTo("Transaction Test Category"));
        }

        [Test]
        public async Task Transaction_RollbackChanges_DiscardChanges()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "Rollback Test Category",
                Description = "Test Description",
                CreatedDate = DateTime.UtcNow
            };
            
            await _unitOfWork.SaveChangesAsync();

            // Assert
            var savedCategory = await _context.Categories.FindAsync(categoryId);
            Assert.That(savedCategory, Is.Null);
        }

        [Test]
        public async Task ComplexTransaction_CommitMultipleOperations()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "Transaction Category",
                Description = "Test Description",
                CreatedDate = DateTime.UtcNow
            };
            _context.Categories.Add(category);

            var bookId = Guid.NewGuid();
            var book = new Book
            {
                BookId = bookId,
                Title = "Transaction Book",
                Author = "Test Author",
                CategoryId = categoryId,
                ISBN = "123456789",
                Publisher = "Test Publisher",
                Description = "Test book description",
                PublishedYear = 2023,
                TotalCopies = 1,
                AvailableCopies = 1,
                IsActive = true
            };
            _context.Books.Add(book);
            await _unitOfWork.SaveChangesAsync();

            // Assert
            var savedCategory = await _context.Categories.FindAsync(categoryId);
            var savedBook = await _context.Books.FindAsync(bookId);
            
            Assert.That(savedCategory, Is.Not.Null);
            Assert.That(savedBook, Is.Not.Null);
            Assert.That(savedBook.CategoryId, Is.EqualTo(savedCategory.CategoryId));
        }

        [Test]
        public void UnitOfWork_ImplementsIDisposable()
        {
            // Arrange & Act & Assert
            Assert.That(_unitOfWork, Is.InstanceOf<IDisposable>());
        }

        [Test]
        public void Dispose_DoesNotThrowException()
        {
            // Arrange & Act & Assert
            Assert.DoesNotThrow(() => _unitOfWork.Dispose());
        }
    }
}
