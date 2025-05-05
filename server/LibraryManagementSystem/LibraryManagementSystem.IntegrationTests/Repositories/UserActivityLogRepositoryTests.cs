using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    public class UserActivityLogRepositoryTests : TestBase
    {
        private UserActivityLogRepository _userActivityLogRepository;
        private Guid _userId;

        public override async Task Setup()
        {
            await base.Setup();
            _userActivityLogRepository = new UserActivityLogRepository(DbContext);
        }

        protected override async Task SeedDataAsync()
        {
            // Create test user
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
            
            _userId = user.UserId;
            await DbContext.Users.AddAsync(user);

            // Create test activity logs
            var logs = new[]
            {
                new UserActivityLog
                {
                    LogId = Guid.NewGuid(),
                    UserId = user.UserId,
                    ActivityType = ActivityType.Login,
                    ActivityDate = DateTime.UtcNow.AddDays(-5),
                    Description = "User logged in"
                },
                new UserActivityLog
                {
                    LogId = Guid.NewGuid(),
                    UserId = user.UserId,
                    ActivityType = ActivityType.BookSearch,
                    ActivityDate = DateTime.UtcNow.AddDays(-4),
                    Description = "User searched for books"
                },
                new UserActivityLog
                {
                    LogId = Guid.NewGuid(),
                    UserId = user.UserId,
                    ActivityType = ActivityType.BookBorrow,
                    ActivityDate = DateTime.UtcNow.AddDays(-3),
                    Description = "User borrowed a book"
                },
                new UserActivityLog
                {
                    LogId = Guid.NewGuid(),
                    UserId = user.UserId,
                    ActivityType = ActivityType.BookReturn,
                    ActivityDate = DateTime.UtcNow.AddDays(-1),
                    Description = "User returned a book"
                }
            };
            
            await DbContext.UserActivityLogs.AddRangeAsync(logs);
            await DbContext.SaveChangesAsync();
        }

        [Test]
        public async Task GetByIdAsync_ExistingLog_ReturnsCorrectLog()
        {
            // Arrange
            var existingLog = await DbContext.UserActivityLogs.FirstAsync();
            
            // Act
            var log = await _userActivityLogRepository.GetByIdAsync(existingLog.LogId);
            
            // Assert
            Assert.That(log, Is.Not.Null);
            Assert.That(log.LogId, Is.EqualTo(existingLog.LogId));
            Assert.That(log.User, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_NonExistingLog_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            // Act
            var log = await _userActivityLogRepository.GetByIdAsync(nonExistingId);
            
            // Assert
            Assert.That(log, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_DefaultParameters_ReturnsPaginatedResults()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            
            // Act
            var logs = (await _userActivityLogRepository.GetAllAsync(pageNumber, pageSize)).ToList();
            
            // Assert
            Assert.That(logs, Is.Not.Null);
            Assert.That(logs.Count, Is.EqualTo(pageSize));
        }

        [Test]
        public async Task GetAllAsync_WithSorting_ReturnsSortedResults()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            
            // Act - Sort by activity date ascending
            var logsAsc = (await _userActivityLogRepository.GetAllAsync(pageNumber, pageSize, "activityDate", "asc")).ToList();
            
            // Assert
            Assert.That(logsAsc, Is.Not.Null);
            Assert.That(logsAsc, Is.Ordered.By("ActivityDate"));
            
            // Act - Sort by activity date descending
            var logsDesc = (await _userActivityLogRepository.GetAllAsync(pageNumber, pageSize, "activityDate", "desc")).ToList();
            
            // Assert
            Assert.That(logsDesc, Is.Not.Null);
            Assert.That(logsDesc, Is.Ordered.Descending.By("ActivityDate"));
        }

        [Test]
        public async Task GetByUserIdAsync_ExistingUser_ReturnsUserLogs()
        {
            // Act
            var logs = (await _userActivityLogRepository.GetByUserIdAsync(_userId, 1, 10)).ToList();
            
            // Assert
            Assert.That(logs, Is.Not.Null);
            Assert.That(logs.Count, Is.EqualTo(4)); // All logs belong to the same user
            Assert.That(logs.All(l => l.UserId == _userId), Is.True);
        }

        [Test]
        public async Task GetByUserIdAsync_NonExistingUser_ReturnsEmptyList()
        {
            // Arrange
            var nonExistingUserId = Guid.NewGuid();
            
            // Act
            var logs = (await _userActivityLogRepository.GetByUserIdAsync(nonExistingUserId, 1, 10)).ToList();
            
            // Assert
            Assert.That(logs, Is.Empty);
        }

        [Test]
        public async Task GetByActivityTypeAsync_ExistingActivityType_ReturnsLogsWithType()
        {
            // Act
            var logs = (await _userActivityLogRepository.GetByActivityTypeAsync(ActivityType.Login, 1, 10)).ToList();
            
            // Assert
            Assert.That(logs, Is.Not.Null);
            Assert.That(logs.Count, Is.EqualTo(1));
            Assert.That(logs.All(l => l.ActivityType == ActivityType.Login), Is.True);
        }

        [Test]
        public async Task GetByActivityTypeAsync_NonExistingActivityType_ReturnsEmptyList()
        {
            // Use an activity type that doesn't exist in the seeded data
            var nonExistingType = (ActivityType)999;
            
            // Act
            var logs = (await _userActivityLogRepository.GetByActivityTypeAsync(nonExistingType, 1, 10)).ToList();
            
            // Assert
            Assert.That(logs, Is.Empty);
        }

        [Test]
        public async Task GetByDateRangeAsync_ValidDateRange_ReturnsLogsInRange()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-4).Date;
            var endDate = DateTime.UtcNow.Date;
            
            // Act
            var logs = (await _userActivityLogRepository.GetByDateRangeAsync(startDate, endDate, 1, 10)).ToList();
            
            // Assert
            Assert.That(logs, Is.Not.Null);
            Assert.That(logs.Count, Is.EqualTo(3)); // Logs from the last 4 days
            Assert.That(logs.All(l => l.ActivityDate.Date >= startDate && l.ActivityDate.Date <= endDate), Is.True);
        }

        [Test]
        public async Task CreateAsync_ValidLog_AddsLogToContext()
        {
            // Arrange
            var newLog = new UserActivityLog
            {
                LogId = Guid.NewGuid(),
                UserId = _userId,
                ActivityType = ActivityType.ProfileUpdate,
                ActivityDate = DateTime.UtcNow,
                Description = "User updated profile"
            };
            
            // Act
            var result = await _userActivityLogRepository.CreateAsync(newLog);
            await DbContext.SaveChangesAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.LogId, Is.EqualTo(newLog.LogId));
            
            // Verify log was added to context
            var addedLog = await DbContext.UserActivityLogs.FindAsync(newLog.LogId);
            Assert.That(addedLog, Is.Not.Null);
            Assert.That(addedLog.ActivityType, Is.EqualTo(ActivityType.ProfileUpdate));
        }

        [Test]
        public async Task DeleteAsync_ExistingLog_RemovesLogFromContext()
        {
            // Arrange
            var existingLog = await DbContext.UserActivityLogs.FirstAsync();
            var logId = existingLog.LogId;
            
            // Act
            await _userActivityLogRepository.DeleteAsync(logId);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var deletedLog = await DbContext.UserActivityLogs.FindAsync(logId);
            Assert.That(deletedLog, Is.Null);
        }

        [Test]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            // Act
            var count = await _userActivityLogRepository.CountAsync();
            
            // Assert
            Assert.That(count, Is.EqualTo(4));
        }

        [Test]
        public async Task CountByUserIdAsync_ExistingUser_ReturnsCorrectCount()
        {
            // Act
            var count = await _userActivityLogRepository.CountByUserIdAsync(_userId);
            
            // Assert
            Assert.That(count, Is.EqualTo(4));
        }

        [Test]
        public async Task CountByActivityTypeAsync_ExistingActivityType_ReturnsCorrectCount()
        {
            // Act
            var count = await _userActivityLogRepository.CountByActivityTypeAsync(ActivityType.Login);
            
            // Assert
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task CountByDateRangeAsync_ValidDateRange_ReturnsCorrectCount()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-4).Date;
            var endDate = DateTime.UtcNow.Date;
            
            // Act
            var count = await _userActivityLogRepository.CountByDateRangeAsync(startDate, endDate);
            
            // Assert
            Assert.That(count, Is.EqualTo(3)); // Logs from the last 4 days
        }
    }
} 