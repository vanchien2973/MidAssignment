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
    public class CategoryRepositoryTests
    {
        private DbContextOptions<LibraryDbContext> _options;
        private LibraryDbContext _context;
        private CategoryRepository _repository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new LibraryDbContext(_options);
            _repository = new CategoryRepository(_context);

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
            var categories = new[]
            {
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Fiction",
                    Description = "Fiction books",
                    CreatedDate = DateTime.UtcNow.AddDays(-30)
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Science",
                    Description = "Science books",
                    CreatedDate = DateTime.UtcNow.AddDays(-25)
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "History",
                    Description = "History books",
                    CreatedDate = DateTime.UtcNow.AddDays(-20)
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Technology",
                    Description = "Technology books",
                    CreatedDate = DateTime.UtcNow.AddDays(-15)
                }
            };
            _context.Categories.AddRange(categories);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsCategory()
        {
            // Arrange
            var categoryId = _context.Categories.First().CategoryId;

            // Act
            var result = await _repository.GetByIdAsync(categoryId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CategoryId, Is.EqualTo(categoryId));
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
        public async Task GetAllAsync_ReturnsAllCategories()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetAllAsync(pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(4));
        }

        [Test]
        public async Task GetAllAsync_WithSearchTerm_ReturnsFilteredCategories()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            string searchTerm = "fic"; // Sẽ tìm "Fiction"

            // Act
            var result = await _repository.GetAllAsync(pageNumber, pageSize, searchTerm: searchTerm);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().CategoryName, Is.EqualTo("Fiction"));
        }

        [Test]
        public async Task GetAllAsync_WithSorting_ReturnsSortedCategories()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            string sortBy = "name";
            string sortOrder = "desc";

            // Act
            var result = await _repository.GetAllAsync(pageNumber, pageSize, sortBy, sortOrder);
            var list = result.ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(4));
            // Kiểm tra sắp xếp giảm dần theo tên
            for (int i = 0; i < list.Count - 1; i++)
            {
                Assert.That(string.Compare(list[i].CategoryName, list[i + 1].CategoryName), Is.GreaterThanOrEqualTo(0));
            }
        }

        [Test]
        public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            int pageNumber = 2;
            int pageSize = 2;

            // Act
            var result = await _repository.GetAllAsync(pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2)); // Trang 2 với kích thước 2 sẽ có 2 mục
        }

        [Test]
        public async Task NameExistsAsync_WithExistingName_ReturnsTrue()
        {
            // Arrange
            string existingName = "Fiction";

            // Act
            var result = await _repository.NameExistsAsync(existingName);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task NameExistsAsync_WithNonExistentName_ReturnsFalse()
        {
            // Arrange
            string nonExistentName = "NonExistentCategory";

            // Act
            var result = await _repository.NameExistsAsync(nonExistentName);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CreateAsync_ValidCategory_AddsToDatabase()
        {
            // Arrange
            var newCategory = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "New Category",
                Description = "New category description",
                CreatedDate = DateTime.UtcNow
            };

            // Act
            var result = await _repository.CreateAsync(newCategory);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            var savedCategory = await _context.Categories.FindAsync(newCategory.CategoryId);
            Assert.That(savedCategory, Is.Not.Null);
            Assert.That(savedCategory.CategoryName, Is.EqualTo("New Category"));
        }

        [Test]
        public async Task UpdateAsync_ExistingCategory_UpdatesInDatabase()
        {
            // Arrange
            var category = await _context.Categories.FirstAsync();
            var originalName = category.CategoryName;
            category.CategoryName = "Updated Category";
            category.Description = "Updated description";

            // Act
            await _repository.UpdateAsync(category);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCategory = await _context.Categories.FindAsync(category.CategoryId);
            Assert.That(updatedCategory, Is.Not.Null);
            Assert.That(updatedCategory.CategoryName, Is.EqualTo("Updated Category"));
            Assert.That(updatedCategory.CategoryName, Is.Not.EqualTo(originalName));
        }

        [Test]
        public async Task DeleteAsync_ExistingCategory_RemovesFromDatabase()
        {
            // Arrange
            var categoryId = _context.Categories.First().CategoryId;

            // Act
            await _repository.DeleteAsync(categoryId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedCategory = await _context.Categories.FindAsync(categoryId);
            Assert.That(deletedCategory, Is.Null);
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
        public async Task CountAsync_WithSearchTerm_ReturnsFilteredCount()
        {
            // Arrange
            string searchTerm = "science";

            // Act
            var result = await _repository.CountAsync(searchTerm);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }
    }
}
