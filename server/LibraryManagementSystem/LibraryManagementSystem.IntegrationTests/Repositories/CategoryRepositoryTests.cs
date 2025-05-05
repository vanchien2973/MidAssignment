using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    public class CategoryRepositoryTests : TestBase
    {
        private CategoryRepository _categoryRepository;

        public override async Task Setup()
        {
            await base.Setup();
            _categoryRepository = new CategoryRepository(DbContext);
        }

        protected override async Task SeedDataAsync()
        {
            // Create test categories
            var categories = new[]
            {
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Fiction",
                    Description = "Fiction books"
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Non-Fiction",
                    Description = "Non-Fiction books"
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Science",
                    Description = "Science books"
                }
            };
            
            await DbContext.Categories.AddRangeAsync(categories);
            await DbContext.SaveChangesAsync();
        }

        [Test]
        public async Task GetByIdAsync_ExistingCategory_ReturnsCorrectCategory()
        {
            // Arrange
            var existingCategory = await DbContext.Categories.FirstAsync();
            
            // Act
            var category = await _categoryRepository.GetByIdAsync(existingCategory.CategoryId);
            
            // Assert
            Assert.That(category, Is.Not.Null);
            Assert.That(category.CategoryId, Is.EqualTo(existingCategory.CategoryId));
            Assert.That(category.CategoryName, Is.EqualTo(existingCategory.CategoryName));
        }

        [Test]
        public async Task GetByIdAsync_NonExistingCategory_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            // Act
            var category = await _categoryRepository.GetByIdAsync(nonExistingId);
            
            // Assert
            Assert.That(category, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_DefaultParameters_ReturnsPaginatedResults()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            
            // Act
            var categories = (await _categoryRepository.GetAllAsync(pageNumber, pageSize)).ToList();
            
            // Assert
            Assert.That(categories, Is.Not.Null);
            Assert.That(categories.Count, Is.EqualTo(pageSize));
        }

        [Test]
        public async Task GetAllAsync_WithSorting_ReturnsSortedResults()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            
            // Act - Sort by name ascending
            var categoriesAsc = (await _categoryRepository.GetAllAsync(pageNumber, pageSize, "name", "asc")).ToList();
            
            // Assert
            Assert.That(categoriesAsc, Is.Not.Null);
            Assert.That(categoriesAsc, Is.Ordered.By("CategoryName"));
            
            // Act - Sort by name descending
            var categoriesDesc = (await _categoryRepository.GetAllAsync(pageNumber, pageSize, "name", "desc")).ToList();
            
            // Assert
            Assert.That(categoriesDesc, Is.Not.Null);
            Assert.That(categoriesDesc, Is.Ordered.Descending.By("CategoryName"));
        }

        [Test]
        public async Task GetByNameAsync_ExistingName_ReturnsCorrectCategory()
        {
            // Act
            var category = await _categoryRepository.GetByNameAsync("Fiction");
            
            // Assert
            Assert.That(category, Is.Not.Null);
            Assert.That(category.CategoryName, Is.EqualTo("Fiction"));
        }

        [Test]
        public async Task GetByNameAsync_NonExistingName_ReturnsNull()
        {
            // Act
            var category = await _categoryRepository.GetByNameAsync("Non-Existing Category");
            
            // Assert
            Assert.That(category, Is.Null);
        }

        [Test]
        public async Task CategoryExistsAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            var existingCategory = await DbContext.Categories.FirstAsync();
            
            // Act
            var exists = await _categoryRepository.CategoryExistsAsync(existingCategory.CategoryId);
            
            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task CategoryExistsAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            // Act
            var exists = await _categoryRepository.CategoryExistsAsync(nonExistingId);
            
            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task CreateAsync_ValidCategory_AddsCategoryToContext()
        {
            // Arrange
            var newCategory = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "New Test Category",
                Description = "New Test Category Description"
            };
            
            // Act
            var result = await _categoryRepository.CreateAsync(newCategory);
            await DbContext.SaveChangesAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CategoryId, Is.EqualTo(newCategory.CategoryId));
            
            // Verify category was added to context
            var addedCategory = await DbContext.Categories.FindAsync(newCategory.CategoryId);
            Assert.That(addedCategory, Is.Not.Null);
            Assert.That(addedCategory.CategoryName, Is.EqualTo("New Test Category"));
        }

        [Test]
        public async Task UpdateAsync_ExistingCategory_UpdatesCategoryInContext()
        {
            // Arrange
            var existingCategory = await DbContext.Categories.FirstAsync();
            existingCategory.CategoryName = "Updated Category Name";
            existingCategory.Description = "Updated Category Description";
            
            // Act
            await _categoryRepository.UpdateAsync(existingCategory);
            await DbContext.SaveChangesAsync();
            
            // Assert - Reload from database to verify changes
            DbContext.Entry(existingCategory).Reload();
            Assert.That(existingCategory.CategoryName, Is.EqualTo("Updated Category Name"));
            Assert.That(existingCategory.Description, Is.EqualTo("Updated Category Description"));
        }

        [Test]
        public async Task DeleteAsync_ExistingCategory_RemovesCategoryFromContext()
        {
            // Arrange
            var existingCategory = await DbContext.Categories.FirstAsync();
            var categoryId = existingCategory.CategoryId;
            
            // Act
            await _categoryRepository.DeleteAsync(categoryId);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var deletedCategory = await DbContext.Categories.FindAsync(categoryId);
            Assert.That(deletedCategory, Is.Null);
        }

        [Test]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            // Act
            var count = await _categoryRepository.CountAsync();
            
            // Assert
            Assert.That(count, Is.EqualTo(3));
        }
    }
} 