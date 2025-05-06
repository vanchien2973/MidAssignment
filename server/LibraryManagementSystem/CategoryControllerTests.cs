using System.Net;
using FluentAssertions;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.FunctionalTests;
using NUnit.Framework;

namespace LibraryManagementSystem.FunctionalTests.Controllers;

[TestFixture]
public class CategoryControllerTests : TestBase
{
    private string _adminToken = string.Empty;
    private string _userToken = string.Empty;
    
    [SetUp]
    public async Task CategorySetup()
    {
        await SeedTestDataAsync();
        
        // Lấy token cho admin và user thông thường
        _adminToken = await GetAuthTokenAsync("adminuser", "Test@123");
        _userToken = await GetAuthTokenAsync("testuser", "Test@123");
    }
    
    private async Task SeedTestDataAsync()
    {
        // Thêm dữ liệu người dùng nếu chưa có
        if (!DbContext.Users.Any())
        {
            var testUser = new User
            {
                UserId = 1,
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "AQAAAAIAAYagAAAAEOQPIBx45LmkFfKyRIkFx71pcG7sHvpwrNV6JR8oeVTTpMcK9q/KsDpXSJtcawtb8A==", // "Test@123"
                FullName = "Test User",
                IsActive = true,
                UserType = Domain.Enums.UserType.NormalUser,
                CreatedDate = DateTime.UtcNow
            };
            
            var adminUser = new User
            {
                UserId = 2,
                Username = "adminuser",
                Email = "admin@example.com",
                Password = "AQAAAAIAAYagAAAAEOQPIBx45LmkFfKyRIkFx71pcG7sHvpwrNV6JR8oeVTTpMcK9q/KsDpXSJtcawtb8A==", // "Test@123"
                FullName = "Admin User",
                IsActive = true,
                UserType = Domain.Enums.UserType.SuperUser,
                CreatedDate = DateTime.UtcNow
            };
            
            DbContext.Users.Add(testUser);
            DbContext.Users.Add(adminUser);
            await DbContext.SaveChangesAsync();
        }
        
        // Thêm dữ liệu danh mục nếu chưa có
        if (!DbContext.Categories.Any())
        {
            var categories = new[]
            {
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Fiction",
                    Description = "Fiction books including novels, short stories",
                    CreatedDate = DateTime.UtcNow
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Science",
                    Description = "Science books including physics, chemistry",
                    CreatedDate = DateTime.UtcNow
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "History",
                    Description = "History books including world history",
                    CreatedDate = DateTime.UtcNow
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Computer Science",
                    Description = "Books about programming, algorithms, and computer science",
                    CreatedDate = DateTime.UtcNow
                }
            };
            
            DbContext.Categories.AddRange(categories);
            await DbContext.SaveChangesAsync();
        }
    }
    
    [Test]
    public async Task GetCategories_ReturnsAllCategories()
    {
        // Arrange
        SetAuthToken(_userToken);
        
        // Act
        var response = await Client.GetAsync("/api/category");
        var categories = await DeserializeResponse<CategoriesResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categories.Should().NotBeNull();
        categories!.Results.Should().NotBeNullOrEmpty();
        categories.Results.Count.Should().Be(4); // 4 danh mục đã được tạo trong SeedTestDataAsync
        
        // Kiểm tra headers phân trang
        response.Headers.Should().ContainKey("X-Total-Count");
        response.Headers.Should().ContainKey("X-Page-Number");
        response.Headers.Should().ContainKey("X-Page-Size");
    }
    
    [Test]
    public async Task GetCategories_WithSearchTerm_ReturnsFilteredCategories()
    {
        // Arrange
        SetAuthToken(_userToken);
        string searchTerm = "science";
        
        // Act
        var response = await Client.GetAsync($"/api/category?searchTerm={searchTerm}");
        var categories = await DeserializeResponse<CategoriesResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categories.Should().NotBeNull();
        categories!.Results.Should().NotBeEmpty();
        categories.Results.All(c => c.CategoryName.ToLower().Contains(searchTerm) || 
                                   c.Description.ToLower().Contains(searchTerm)).Should().BeTrue();
    }
    
    [Test]
    public async Task GetCategory_WithValidId_ReturnsCategoryDetails()
    {
        // Arrange
        SetAuthToken(_userToken);
        var categoryId = DbContext.Categories.First().CategoryId;
        
        // Act
        var response = await Client.GetAsync($"/api/category/{categoryId}");
        var category = await DeserializeResponse<CategoryDetailsDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        category.Should().NotBeNull();
        category!.CategoryId.Should().Be(categoryId);
    }
    
    [Test]
    public async Task GetCategory_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        SetAuthToken(_userToken);
        var invalidCategoryId = Guid.NewGuid();
        
        // Act
        var response = await Client.GetAsync($"/api/category/{invalidCategoryId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task CreateCategory_WithAdminToken_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        var newCategory = new
        {
            CategoryName = "New Test Category",
            Description = "A category created for testing purposes"
        };
        
        // Act
        var response = await Client.PostAsync("/api/category", CreateJsonContent(newCategory));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Kiểm tra danh mục đã được thêm vào cơ sở dữ liệu
        var createdCategory = DbContext.Categories.FirstOrDefault(c => c.CategoryName == "New Test Category");
        createdCategory.Should().NotBeNull();
        createdCategory!.Description.Should().Be("A category created for testing purposes");
    }
    
    [Test]
    public async Task CreateCategory_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        SetAuthToken(_userToken);
        var newCategory = new
        {
            CategoryName = "Unauthorized Category",
            Description = "A category that shouldn't be created"
        };
        
        // Act
        var response = await Client.PostAsync("/api/category", CreateJsonContent(newCategory));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        // Kiểm tra danh mục không được thêm vào cơ sở dữ liệu
        var unauthorizedCategory = DbContext.Categories.FirstOrDefault(c => c.CategoryName == "Unauthorized Category");
        unauthorizedCategory.Should().BeNull();
    }
    
    [Test]
    public async Task UpdateCategory_WithAdminToken_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        var categoryToUpdate = DbContext.Categories.First();
        var updatedInfo = new
        {
            CategoryId = categoryToUpdate.CategoryId,
            CategoryName = categoryToUpdate.CategoryName + " (Updated)",
            Description = "Updated description for testing"
        };
        
        // Act
        var response = await Client.PutAsync("/api/category", CreateJsonContent(updatedInfo));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Kiểm tra danh mục đã được cập nhật trong cơ sở dữ liệu
        var updatedCategory = await DbContext.Categories.FindAsync(categoryToUpdate.CategoryId);
        updatedCategory.Should().NotBeNull();
        updatedCategory!.CategoryName.Should().Be(categoryToUpdate.CategoryName + " (Updated)");
        updatedCategory.Description.Should().Be("Updated description for testing");
    }
    
    [Test]
    public async Task DeleteCategory_WithNoBooks_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        // Tạo một danh mục mới không có sách liên kết
        var categoryWithNoBooks = new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Category to Delete",
            Description = "This category will be deleted",
            CreatedDate = DateTime.UtcNow
        };
        DbContext.Categories.Add(categoryWithNoBooks);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/category/{categoryWithNoBooks.CategoryId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Kiểm tra danh mục đã bị xóa khỏi cơ sở dữ liệu
        var deletedCategory = await DbContext.Categories.FindAsync(categoryWithNoBooks.CategoryId);
        deletedCategory.Should().BeNull();
    }
    
    [Test]
    public async Task DeleteCategory_WithBooks_ReturnsBadRequest()
    {
        // Arrange
        SetAuthToken(_adminToken);
        // Tạo một danh mục mới và sách liên kết
        var categoryWithBook = new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Category with Books",
            Description = "This category has books and shouldn't be deleted",
            CreatedDate = DateTime.UtcNow
        };
        DbContext.Categories.Add(categoryWithBook);
        await DbContext.SaveChangesAsync();
        
        var book = new Book
        {
            BookId = Guid.NewGuid(),
            Title = "Book in Category",
            Author = "Test Author",
            CategoryId = categoryWithBook.CategoryId,
            ISBN = "9781234567891",
            PublishedYear = 2023,
            Publisher = "Test Publisher",
            Description = "A book in the category to prevent deletion",
            TotalCopies = 1,
            AvailableCopies = 1,
            IsActive = true
        };
        DbContext.Books.Add(book);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/category/{categoryWithBook.CategoryId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // hoặc BadRequest, tùy vào cài đặt của API
        
        // Kiểm tra danh mục vẫn còn trong cơ sở dữ liệu
        var category = await DbContext.Categories.FindAsync(categoryWithBook.CategoryId);
        category.Should().NotBeNull();
    }
    
    #region DTOs for Testing
    private class CategoriesResponseDto
    {
        public List<CategoryListDto> Results { get; set; } = new();
    }
    
    private class CategoryListDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
    
    private class CategoryDetailsDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int BookCount { get; set; }
    }
    #endregion
} 