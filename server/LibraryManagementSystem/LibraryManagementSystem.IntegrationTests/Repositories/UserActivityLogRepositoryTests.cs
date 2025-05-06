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
    public class UserActivityLogRepositoryTests
    {
        private DbContextOptions<LibraryDbContext> _options;
        private LibraryDbContext _context;
        private UserActivityLogRepository _repository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new LibraryDbContext(_options);
            _repository = new UserActivityLogRepository(_context);

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
            var user1 = new User
            {
                UserId = 1,
                Username = "user1",
                FullName = "User One",
                Email = "user1@example.com",
                Password = "hashed_password",
                IsActive = true
            };

            var user2 = new User
            {
                UserId = 2,
                Username = "user2",
                FullName = "User Two",
                Email = "user2@example.com",
                Password = "hashed_password",
                IsActive = true
            };
            _context.Users.AddRange(user1, user2);

            var logs = new[]
            {
                new UserActivityLog
                {
                    LogId = 1,
                    UserId = user1.UserId,
                    ActivityType = "Login",
                    ActivityDate = DateTime.UtcNow.AddDays(-2),
                    Details = "User logged in",
                    IpAddress = "192.168.1.1"
                },
                new UserActivityLog
                {
                    LogId = 2,
                    UserId = user1.UserId,
                    ActivityType = "BookView",
                    ActivityDate = DateTime.UtcNow.AddDays(-1),
                    Details = "User viewed book details",
                    IpAddress = "192.168.1.1"
                },
                new UserActivityLog
                {
                    LogId = 3,
                    UserId = user1.UserId,
                    ActivityType = "Logout",
                    ActivityDate = DateTime.UtcNow.AddHours(-12),
                    Details = "User logged out",
                    IpAddress = "192.168.1.1"
                },
                new UserActivityLog
                {
                    LogId = 4,
                    UserId = user2.UserId,
                    ActivityType = "Login",
                    ActivityDate = DateTime.UtcNow.AddHours(-6),
                    Details = "User logged in",
                    IpAddress = "192.168.1.2"
                },
                new UserActivityLog
                {
                    LogId = 5,
                    UserId = user2.UserId,
                    ActivityType = "BorrowRequest",
                    ActivityDate = DateTime.UtcNow.AddHours(-5),
                    Details = "User requested to borrow a book",
                    IpAddress = "192.168.1.2"
                }
            };
            _context.UserActivityLogs.AddRange(logs);
            _context.SaveChanges();
        }

        [Test]
        public async Task LogActivityAsync_ValidLog_AddsToDatabase()
        {
            // Arrange
            var newLog = new UserActivityLog
            {
                UserId = 1,
                ActivityType = "ProfileUpdate",
                ActivityDate = DateTime.UtcNow,
                Details = "User updated profile",
                IpAddress = "192.168.1.1"
            };

            // Act
            var result = await _repository.LogActivityAsync(newLog);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.LogId, Is.GreaterThan(0));
            var savedLog = await _context.UserActivityLogs.FindAsync(result.LogId);
            Assert.That(savedLog, Is.Not.Null);
            Assert.That(savedLog.ActivityType, Is.EqualTo("ProfileUpdate"));
        }

        [Test]
        public async Task GetUserActivityLogsAsync_ExistingUserId_ReturnsUserLogs()
        {
            // Arrange
            int userId = 1;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetUserActivityLogsAsync(userId, null, pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
            foreach (var log in result)
            {
                Assert.That(log.UserId, Is.EqualTo(userId));
            }
        }

        [Test]
        public async Task GetUserActivityLogsAsync_WithActivityType_ReturnsFilteredLogs()
        {
            // Arrange
            int userId = 1;
            string activityType = "Login";
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetUserActivityLogsAsync(userId, activityType, pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().ActivityType, Is.EqualTo(activityType));
        }

        [Test]
        public async Task GetUserActivityLogsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            int userId = 1;
            int pageNumber = 2;
            int pageSize = 1;

            // Act
            var result = await _repository.GetUserActivityLogsAsync(userId, null, pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            // Trang 2 với kích thước 1 sẽ chứa log thứ hai
            Assert.That(result.First().LogId, Is.EqualTo(2));
        }
        
        [Test]
        public async Task GetUserActivityLogsAsync_OrderedByActivityDateDescending()
        {
            // Arrange
            int userId = 1;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetUserActivityLogsAsync(userId, null, pageNumber, pageSize);
            var list = result.ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            // Kiểm tra sắp xếp giảm dần theo ActivityDate
            for (int i = 0; i < list.Count - 1; i++)
            {
                Assert.That(list[i].ActivityDate, Is.GreaterThanOrEqualTo(list[i + 1].ActivityDate));
            }
        }
    }
}
