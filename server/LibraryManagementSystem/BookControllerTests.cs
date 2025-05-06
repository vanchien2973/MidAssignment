using System.Net;
using FluentAssertions;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.FunctionalTests;
using NUnit.Framework;

namespace LibraryManagementSystem.FunctionalTests.Controllers;

[TestFixture]
public class BookControllerTests : TestBase
{
    private string _adminToken = string.Empty;
    private string _userToken = string.Empty;
    private Guid _testCategoryId = Guid.NewGuid();
    
    [SetUp]
    public async Task BookSetup()
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
                    CategoryId = _testCategoryId,
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
                }
            };
            
            DbContext.Categories.AddRange(categories);
            await DbContext.SaveChangesAsync();
        }
        
        // Thêm dữ liệu sách nếu chưa có
        if (!DbContext.Books.Any())
        {
            var books = new[]
            {
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "The Great Gatsby",
                    Author = "F. Scott Fitzgerald",
                    CategoryId = _testCategoryId,
                    ISBN = "9780743273565",
                    PublishedYear = 1925,
                    Publisher = "Scribner",
                    Description = "The story of eccentric millionaire Jay Gatsby",
                    TotalCopies = 5,
                    AvailableCopies = 3,
                    IsActive = true
                },
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "To Kill a Mockingbird",
                    Author = "Harper Lee",
                    CategoryId = _testCategoryId,
                    ISBN = "9780061120084",
                    PublishedYear = 1960,
                    Publisher = "HarperCollins",
                    Description = "The story of Scout Finch and her father Atticus",
                    TotalCopies = 10,
                    AvailableCopies = 8,
                    IsActive = true
                },
                new Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "A Brief History of Time",
                    Author = "Stephen Hawking",
                    CategoryId = DbContext.Categories.FirstOrDefault(c => c.CategoryName == "Science")?.CategoryId ?? Guid.NewGuid(),
                    ISBN = "9780553380163",
                    PublishedYear = 1988,
                    Publisher = "Bantam Books",
                    Description = "A book on cosmology by Stephen Hawking",
                    TotalCopies = 7,
                    AvailableCopies = 4,
                    IsActive = true
                }
            };
            
            DbContext.Books.AddRange(books);
            await DbContext.SaveChangesAsync();
        }
    }
    
    [Test]
    public async Task GetBooks_ReturnsAllBooks()
    {
        // Arrange
        SetAuthToken(_userToken);
        
        // Act
        var response = await Client.GetAsync("/api/book");
        var books = await DeserializeResponse<BooksResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        books.Should().NotBeNull();
        books!.Results.Should().NotBeNullOrEmpty();
        books.Results.Count.Should().Be(3); // 3 sách đã được tạo trong SeedTestDataAsync
        
        // Kiểm tra headers phân trang
        response.Headers.Should().ContainKey("X-Total-Count");
        response.Headers.Should().ContainKey("X-Page-Number");
        response.Headers.Should().ContainKey("X-Page-Size");
    }
    
    [Test]
    public async Task GetBook_WithValidId_ReturnsBookDetails()
    {
        // Arrange
        SetAuthToken(_userToken);
        var bookId = DbContext.Books.First().BookId;
        
        // Act
        var response = await Client.GetAsync($"/api/book/{bookId}");
        var book = await DeserializeResponse<BookDetailsDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        book.Should().NotBeNull();
        book!.BookId.Should().Be(bookId);
    }
    
    [Test]
    public async Task GetBook_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        SetAuthToken(_userToken);
        var invalidBookId = Guid.NewGuid();
        
        // Act
        var response = await Client.GetAsync($"/api/book/{invalidBookId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task GetBooksByCategory_ReturnsBooksInCategory()
    {
        // Arrange
        SetAuthToken(_userToken);
        
        // Act
        var response = await Client.GetAsync($"/api/book/by-category/{_testCategoryId}");
        var books = await DeserializeResponse<BooksResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        books.Should().NotBeNull();
        books!.Results.Should().NotBeNullOrEmpty();
        books.Results.All(b => b.CategoryId == _testCategoryId).Should().BeTrue();
    }
    
    [Test]
    public async Task GetAvailableBooks_ReturnsOnlyAvailableBooks()
    {
        // Arrange
        SetAuthToken(_userToken);
        
        // Act
        var response = await Client.GetAsync("/api/book/available");
        var books = await DeserializeResponse<BooksResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        books.Should().NotBeNull();
        books!.Results.Should().NotBeNullOrEmpty();
        books.Results.All(b => b.AvailableCopies > 0).Should().BeTrue();
    }
    
    [Test]
    public async Task CreateBook_WithAdminToken_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        var newBook = new
        {
            Title = "New Test Book",
            Author = "Test Author",
            CategoryId = _testCategoryId,
            ISBN = "9781234567890",
            PublishedYear = 2023,
            Publisher = "Test Publisher",
            Description = "A book created for testing",
            TotalCopies = 3,
            AvailableCopies = 3
        };
        
        // Act
        var response = await Client.PostAsync("/api/book", CreateJsonContent(newBook));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Kiểm tra sách đã được thêm vào cơ sở dữ liệu
        var createdBook = DbContext.Books.FirstOrDefault(b => b.Title == "New Test Book");
        createdBook.Should().NotBeNull();
        createdBook!.Author.Should().Be("Test Author");
        createdBook.ISBN.Should().Be("9781234567890");
    }
    
    [Test]
    public async Task CreateBook_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        SetAuthToken(_userToken);
        var newBook = new
        {
            Title = "Unauthorized Book",
            Author = "Unauthorized Author",
            CategoryId = _testCategoryId,
            ISBN = "9780987654321",
            PublishedYear = 2023,
            Publisher = "Test Publisher",
            Description = "A book that shouldn't be created",
            TotalCopies = 3,
            AvailableCopies = 3
        };
        
        // Act
        var response = await Client.PostAsync("/api/book", CreateJsonContent(newBook));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        // Kiểm tra sách không được thêm vào cơ sở dữ liệu
        var unauthorizedBook = DbContext.Books.FirstOrDefault(b => b.Title == "Unauthorized Book");
        unauthorizedBook.Should().BeNull();
    }
    
    [Test]
    public async Task UpdateBook_WithAdminToken_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        var bookToUpdate = DbContext.Books.First();
        var updatedInfo = new
        {
            BookId = bookToUpdate.BookId,
            Title = bookToUpdate.Title + " (Updated)",
            Author = bookToUpdate.Author,
            CategoryId = bookToUpdate.CategoryId,
            ISBN = bookToUpdate.ISBN,
            PublishedYear = bookToUpdate.PublishedYear,
            Publisher = bookToUpdate.Publisher,
            Description = "Updated description for testing",
            TotalCopies = bookToUpdate.TotalCopies + 2,
            AvailableCopies = bookToUpdate.AvailableCopies + 2
        };
        
        // Act
        var response = await Client.PutAsync("/api/book", CreateJsonContent(updatedInfo));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Kiểm tra sách đã được cập nhật trong cơ sở dữ liệu
        var updatedBook = await DbContext.Books.FindAsync(bookToUpdate.BookId);
        updatedBook.Should().NotBeNull();
        updatedBook!.Title.Should().Be(bookToUpdate.Title + " (Updated)");
        updatedBook.Description.Should().Be("Updated description for testing");
        updatedBook.TotalCopies.Should().Be(bookToUpdate.TotalCopies + 2);
    }
    
    [Test]
    public async Task DeleteBook_WithAdminToken_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        var bookToDelete = DbContext.Books.First();
        
        // Act
        var response = await Client.DeleteAsync($"/api/book/{bookToDelete.BookId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Kiểm tra sách đã bị xóa khỏi cơ sở dữ liệu
        var deletedBook = await DbContext.Books.FindAsync(bookToDelete.BookId);
        deletedBook.Should().BeNull();
    }
    
    #region DTOs for Testing
    private class BooksResponseDto
    {
        public List<BookListDto> Results { get; set; } = new();
    }
    
    private class BookListDto
    {
        public Guid BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public bool IsActive { get; set; }
    }
    
    private class BookDetailsDto
    {
        public Guid BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public bool IsActive { get; set; }
    }
    #endregion
} 