using System.Net;
using FluentAssertions;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.FunctionalTests.Controllers;

[TestFixture]
public class AdminControllerTests : TestBase
{
    private string _adminToken = string.Empty;
    private string _userToken = string.Empty;
    
    [SetUp]
    public async Task AdminSetup()
    {
        await SeedTestUsersAsync();
        
        // Lấy token cho admin và user thông thường
        _adminToken = await GetAuthTokenAsync("adminuser", "Test@123");
        _userToken = await GetAuthTokenAsync("testuser", "Test@123");
    }
    
    private async Task SeedTestUsersAsync()
    {
        // Thêm dữ liệu người dùng
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
            
            var inactiveUser = new User
            {
                UserId = 3,
                Username = "inactiveuser",
                Email = "inactive@example.com",
                Password = "AQAAAAIAAYagAAAAEOQPIBx45LmkFfKyRIkFx71pcG7sHvpwrNV6JR8oeVTTpMcK9q/KsDpXSJtcawtb8A==", // "Test@123"
                FullName = "Inactive User",
                IsActive = false,
                UserType = Domain.Enums.UserType.NormalUser,
                CreatedDate = DateTime.UtcNow
            };
            
            DbContext.Users.Add(testUser);
            DbContext.Users.Add(adminUser);
            DbContext.Users.Add(inactiveUser);
            
            await DbContext.SaveChangesAsync();
            
            // Thêm log hoạt động người dùng
            var activityLogs = new[]
            {
                new UserActivityLog
                {
                    LogId = 1,
                    UserId = 1,
                    ActivityType = "Login",
                    Details = "User logged in",
                    ActivityDate = DateTime.UtcNow.AddDays(-1),
                    IpAddress = "127.0.0.1"
                },
                new UserActivityLog
                {
                    LogId = 2,
                    UserId = 1,
                    ActivityType = "BorrowRequest",
                    Details = "User created a borrow request",
                    ActivityDate = DateTime.UtcNow.AddHours(-12),
                    IpAddress = "127.0.0.1"
                },
                new UserActivityLog
                {
                    LogId = 3,
                    UserId = 2,
                    ActivityType = "Login",
                    Details = "Admin logged in",
                    ActivityDate = DateTime.UtcNow.AddHours(-6),
                    IpAddress = "127.0.0.1"
                }
            };
            
            DbContext.UserActivityLogs.AddRange(activityLogs);
            await DbContext.SaveChangesAsync();
        }
    }
    
    [Test]
    public async Task GetAllUsers_WithAdminToken_ReturnsAllUsers()
    {
        // Arrange
        SetAuthToken(_adminToken);
        
        // Act
        var response = await Client.GetAsync("/api/admin/users");
        var users = await DeserializeResponse<UsersResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        users.Should().NotBeNull();
        users!.Results.Should().NotBeNullOrEmpty();
        users.Results.Count.Should().Be(3); // 3 người dùng đã được tạo trong SeedTestUsersAsync
    }
    
    [Test]
    public async Task GetAllUsers_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        SetAuthToken(_userToken);
        
        // Act
        var response = await Client.GetAsync("/api/admin/users");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Test]
    public async Task GetUserById_WithAdminToken_ReturnsUserDetails()
    {
        // Arrange
        SetAuthToken(_adminToken);
        
        // Act
        var response = await Client.GetAsync("/api/admin/users/1");
        var userResponse = await DeserializeResponse<UserResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        userResponse.Should().NotBeNull();
        userResponse!.Success.Should().BeTrue();
        userResponse.User.Should().NotBeNull();
        userResponse.User!.UserId.Should().Be(1);
        userResponse.User!.Username.Should().Be("testuser");
    }
    
    [Test]
    public async Task GetUserById_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        SetAuthToken(_adminToken);
        
        // Act
        var response = await Client.GetAsync("/api/admin/users/999");
        var userResponse = await DeserializeResponse<UserResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        userResponse.Should().NotBeNull();
        userResponse!.Success.Should().BeFalse();
    }
    
    [Test]
    public async Task UpdateUserRole_ToSuperUser_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        var updateRoleDto = new { UserType = "Admin" };
        
        // Act
        var response = await Client.PutAsync("/api/admin/users/1/role", CreateJsonContent(updateRoleDto));
        var responseContent = await DeserializeResponse<UserResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        
        // Kiểm tra vai trò đã được cập nhật trong cơ sở dữ liệu
        var updatedUser = await DbContext.Users.FindAsync(1);
        updatedUser.Should().NotBeNull();
        updatedUser!.UserType.Should().Be(Domain.Enums.UserType.SuperUser);
    }
    
    [Test]
    public async Task ActivateUser_InactiveUser_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        
        // Act
        var response = await Client.PostAsync("/api/admin/users/3/activate", null);
        var responseContent = await DeserializeResponse<UserResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        
        // Kiểm tra người dùng đã được kích hoạt trong cơ sở dữ liệu
        var activatedUser = await DbContext.Users.FindAsync(3);
        activatedUser.Should().NotBeNull();
        activatedUser!.IsActive.Should().BeTrue();
    }
    
    [Test]
    public async Task DeactivateUser_ActiveUser_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        
        // Act
        var response = await Client.PostAsync("/api/admin/users/1/deactivate", null);
        var responseContent = await DeserializeResponse<UserResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        
        // Kiểm tra người dùng đã bị vô hiệu hóa trong cơ sở dữ liệu
        var deactivatedUser = await DbContext.Users.FindAsync(1);
        deactivatedUser.Should().NotBeNull();
        deactivatedUser!.IsActive.Should().BeFalse();
    }
    
    [Test]
    public async Task DeleteUser_NormalUser_Success()
    {
        // Arrange
        SetAuthToken(_adminToken);
        
        // Act
        var response = await Client.DeleteAsync("/api/admin/users/1");
        var responseContent = await DeserializeResponse<UserResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        
        // Kiểm tra người dùng đã bị xóa khỏi cơ sở dữ liệu
        var deletedUser = await DbContext.Users.FindAsync(1);
        deletedUser.Should().BeNull();
    }
    
    [Test]
    public async Task GetUserActivityLogs_WithAdminToken_ReturnsLogs()
    {
        // Arrange
        SetAuthToken(_adminToken);
        
        // Act
        var response = await Client.GetAsync("/api/admin/users/activity-logs?userId=1");
        var logs = await DeserializeResponse<ActivityLogsResponseDto>(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        logs.Should().NotBeNull();
        logs!.Results.Should().NotBeNullOrEmpty();
        logs.Results.Count.Should().Be(2); // 2 logs cho người dùng có UserId = 1
        logs.Results.All(log => log.UserId == 1).Should().BeTrue();
    }
    
    #region DTOs for Testing
    private class UsersResponseDto
    {
        public List<UserDto> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
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
    }
    #endregion
} 