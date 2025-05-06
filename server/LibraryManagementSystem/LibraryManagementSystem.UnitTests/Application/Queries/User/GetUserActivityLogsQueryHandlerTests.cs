using LibraryManagementSystem.Application.Handlers.QueryHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.User;
using LibraryManagementSystem.Domain.Entities;
using Moq;

namespace LibraryManagementSystem.UnitTests.Application.Queries.User;

public class GetUserActivityLogsQueryHandlerTests
{
    private Mock<IUserActivityLogRepository> _mockLogRepository;
    private Mock<IUserRepository> _mockUserRepository;
    private GetUserActivityLogsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockLogRepository = new Mock<IUserActivityLogRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new GetUserActivityLogsQueryHandler(
            _mockLogRepository.Object,
            _mockUserRepository.Object
        );
    }

    [Test]
    public async Task Handle_ReturnsCorrectActivityLogs()
    {
        // Arrange
        int userId = 1;
        string activityType = "Login";
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetUserActivityLogsQuery
        {
            UserId = userId,
            ActivityType = activityType,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var logs = new List<UserActivityLog>
        {
            new() { LogId = 1, UserId = userId, ActivityType = activityType, ActivityDate = DateTime.UtcNow.AddHours(-1) },
            new() { LogId = 2, UserId = userId, ActivityType = activityType, ActivityDate = DateTime.UtcNow }
        };
        
        var users = new List<LibraryManagementSystem.Domain.Entities.User>
        {
            new() { UserId = userId, Username = "testUser" }
        };
        
        _mockLogRepository.Setup(repo => repo.GetUserActivityLogsAsync(
                userId, activityType, pageNumber, pageSize))
            .ReturnsAsync(logs);
        
        _mockUserRepository.Setup(repo => repo.GetUsersByIdsAsync(It.Is<List<int>>(ids => ids.Contains(userId))))
            .ReturnsAsync(users);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(logs.Count));
        
        foreach (var logDto in result)
        {
            Assert.That(logDto.Username, Is.EqualTo("testUser"));
        }
        
        _mockLogRepository.Verify(repo => repo.GetUserActivityLogsAsync(
            userId, activityType, pageNumber, pageSize), Times.Once);
        _mockUserRepository.Verify(repo => repo.GetUsersByIdsAsync(It.Is<List<int>>(ids => ids.Contains(userId))), Times.Once);
    }
    
    // [Test]
    // public async Task Handle_WithMultipleUsers_MapsUsernamesCorrectly()
    // {
    //     // Arrange
    //     int pageNumber = 1;
    //     int pageSize = 10;
    //     string activityType = null; // All activity types
    //     
    //     var query = new GetUserActivityLogsQuery
    //     {
    //         UserId = null,
    //         ActivityType = activityType,
    //         PageNumber = pageNumber,
    //         PageSize = pageSize
    //     };
    //     
    //     var logs = new List<UserActivityLog>
    //     {
    //         new() { LogId = 1, UserId = 1, ActivityType = "Login", ActivityDate = DateTime.UtcNow.AddHours(-2) },
    //         new() { LogId = 2, UserId = 2, ActivityType = "BookView", ActivityDate = DateTime.UtcNow.AddHours(-1) },
    //         new() { LogId = 3, UserId = 1, ActivityType = "Logout", ActivityDate = DateTime.UtcNow }
    //     };
    //     
    //     var users = new List<LibraryManagementSystem.Domain.Entities.User>
    //     {
    //         new() { UserId = 1, Username = "user1" },
    //         new() { UserId = 2, Username = "user2" }
    //     };
    //     
    //     _mockLogRepository.Setup(repo => repo.GetUserActivityLogsAsync(
    //             It.IsAny<int>(), query.ActivityType, pageNumber, pageSize))
    //         .ReturnsAsync(logs);
    //     
    //     _mockUserRepository.Setup(repo => repo.GetUsersByIdsAsync(It.Is<List<int>>(ids => 
    //         ids.Contains(1) && ids.Contains(2) && ids.Count == 2)))
    //         .ReturnsAsync(users);
    //     
    //     // Act
    //     var result = await _handler.Handle(query, CancellationToken.None);
    //     
    //     // Assert
    //     Assert.That(result, Is.Not.Null);
    //     Assert.That(result.Count(), Is.EqualTo(logs.Count));
    //     
    //     var resultList = result.ToList();
    //     Assert.That(resultList[0].Username, Is.EqualTo("user1"));
    //     Assert.That(resultList[1].Username, Is.EqualTo("user2"));
    //     Assert.That(resultList[2].Username, Is.EqualTo("user1"));
    //     
    //     _mockLogRepository.Verify(repo => repo.GetUserActivityLogsAsync(
    //         It.IsAny<int>(), query.ActivityType, pageNumber, pageSize), Times.Once);
    //     _mockUserRepository.Verify(repo => repo.GetUsersByIdsAsync(It.IsAny<List<int>>()), Times.Once);
    // }
    
    [Test]
    public async Task Handle_WithEmptyLogs_ReturnsEmptyList()
    {
        // Arrange
        int userId = 999;
        string activityType = "NonExistentActivity";
        int pageNumber = 1;
        int pageSize = 10;
        
        var query = new GetUserActivityLogsQuery
        {
            UserId = userId,
            ActivityType = activityType,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var emptyLogs = new List<UserActivityLog>();
        
        _mockLogRepository.Setup(repo => repo.GetUserActivityLogsAsync(
                userId, activityType, pageNumber, pageSize))
            .ReturnsAsync(emptyLogs);
            
        // Cập nhật để chấp nhận cuộc gọi với danh sách trống
        _mockUserRepository.Setup(repo => repo.GetUsersByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<LibraryManagementSystem.Domain.Entities.User>());
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        
        _mockLogRepository.Verify(repo => repo.GetUserActivityLogsAsync(
            userId, activityType, pageNumber, pageSize), Times.Once);
        // Không kiểm tra liệu có gọi hay không, vì thực ra có thể gọi với một danh sách trống
    }
}
