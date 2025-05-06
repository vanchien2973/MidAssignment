using System.Net;
using FluentAssertions;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.FunctionalTests.Controllers;

[TestFixture]
public class UserControllerTests : TestBase
{
    private string _userToken = string.Empty;
    
    [SetUp]
    public async Task UserSetup()
    {
        await SeedTestDataAsync();
        
        // Lấy token cho user thông thường
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
        
        // Thêm dữ liệu hoạt động người dùng
        if (!DbContext.UserActivityLogs.Any())
        {
            var log1 = new UserActivityLog
            {
                LogId = 1,
                UserId = 1,
                ActivityType = "Login",
                Details = "User logged in",
                ActivityDate = DateTime.UtcNow.AddDays(-2),
                IpAddress = "127.0.0.1"
            };
            
            var log2 = new UserActivityLog
            {
                LogId = 2,
                UserId = 1,
                ActivityType = "PasswordChange",
                Details = "User changed password",
                ActivityDate = DateTime.UtcNow.AddDays(-1),
                IpAddress = "127.0.0.1"
            };
            
            DbContext.UserActivityLogs.AddRange(log1, log2);
            await DbContext.SaveChangesAsync();
        }
    }
    
    [Test]
    public async Task GetProfile_WithValidToken_ReturnsUserProfile()
    {
        // Arrange
        SetAuthToken(_userToken);
        
        // Act
        var response = await Client.GetAsync("/api/user/profile");
        var profile = await DeserializeResponse<UserResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        profile.Should().NotBeNull();
        profile!.Success.Should().BeTrue();
        profile.User.Should().NotBeNull();
        profile.User!.UserId.Should().Be(1);
        profile.User.Username.Should().Be("testuser");
    }
    
    [Test]
    public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange - không set token
        
        // Act
        var response = await Client.GetAsync("/api/user/profile");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Test]
    public async Task GetUserActivityLogs_ReturnsUserLogs()
    {
        // Arrange
        SetAuthToken(_userToken);
        
        // Act
        var response = await Client.GetAsync("/api/user/activity-logs");
        var logs = await DeserializeResponse<ActivityLogsResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        logs.Should().NotBeNull();
        logs!.Results.Should().NotBeNullOrEmpty();
        logs.Results.Count.Should().Be(2); // Hai log đã được tạo trong SeedTestDataAsync
        logs.Results.All(log => log.UserId == 1).Should().BeTrue();
        logs.Results[0].Details.Should().Be("User logged in");
        logs.Results[0].ActivityDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(-2), TimeSpan.FromSeconds(1));
        logs.Results[1].Details.Should().Be("User changed password");
        logs.Results[1].ActivityDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(-1), TimeSpan.FromSeconds(1));
    }
    
    [Test]
    public async Task UpdateUserProfile_Success()
    {
        // Arrange
        SetAuthToken(_userToken);
        var profileUpdate = new
        {
            FullName = "Updated User",
            Email = "updated@example.com"
        };
        
        // Act
        var response = await Client.PutAsync("/api/user/profile", CreateJsonContent(profileUpdate));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Kiểm tra thông tin người dùng đã được cập nhật
        var updatedUser = await DbContext.Users.FindAsync(1);
        updatedUser.Should().NotBeNull();
        updatedUser!.FullName.Should().Be("Updated User");
        updatedUser.Email.Should().Be("updated@example.com");
    }
    
    [Test]
    public async Task UpdatePassword_WithValidData_Success()
    {
        // Arrange
        SetAuthToken(_userToken);
        var updatePasswordDto = new
        {
            CurrentPassword = "Test@123",
            NewPassword = "NewPass@123",
            ConfirmPassword = "NewPass@123"
        };
        
        // Act
        var response = await Client.PutAsync("/api/user/password", CreateJsonContent(updatePasswordDto));
        var result = await DeserializeResponse<UserResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        
        // Kiểm tra mật khẩu đã được cập nhật - lưu ý là trong test chúng ta không thể kiểm tra mật khẩu đã hash
        // Thay vào đó, có thể thử đăng nhập với mật khẩu mới
        var loginData = new
        {
            Username = "testuser",
            Password = "NewPass@123"
        };
        
        var loginResponse = await Client.PostAsync("/api/auth/login", CreateJsonContent(loginData));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task UpdatePassword_WithIncorrectCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        SetAuthToken(_userToken);
        var updatePasswordDto = new
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPass@123",
            ConfirmPassword = "NewPass@123"
        };
        
        // Act
        var response = await Client.PutAsync("/api/user/password", CreateJsonContent(updatePasswordDto));
        var result = await DeserializeResponse<UserResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }
    
    #region DTOs for Testing
    private class UserResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
    }
    
    private class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string UserType { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
    
    private class ActivityLogsResponseDto
    {
        public List<ActivityLogDto> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
    private class ActivityLogDto
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        public string Username { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }
    #endregion
} 