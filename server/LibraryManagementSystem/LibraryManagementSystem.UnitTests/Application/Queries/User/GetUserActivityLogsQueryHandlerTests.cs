using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.User;
using LibraryManagementSystem.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Queries.User
{
    [TestFixture]
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
            _handler = new GetUserActivityLogsQueryHandler(_mockLogRepository.Object, _mockUserRepository.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnUserActivityLogs()
        {
            // Arrange
            var userId = 1;
            var activityType = "LOGIN";
            var pageNumber = 1;
            var pageSize = 10;

            var logs = new List<UserActivityLog>
            {
                new UserActivityLog
                {
                    LogId = 1,
                    UserId = userId,
                    ActivityType = "LOGIN",
                    ActivityDate = DateTime.UtcNow.AddDays(-1),
                    Details = "User logged in",
                    IpAddress = "192.168.1.1"
                },
                new UserActivityLog
                {
                    LogId = 2,
                    UserId = userId,
                    ActivityType = "PROFILE_UPDATE",
                    ActivityDate = DateTime.UtcNow.AddDays(-2),
                    Details = "User updated profile",
                    IpAddress = "192.168.1.1"
                }
            };

            var users = new List<Domain.Entities.User>
            {
                new Domain.Entities.User
                {
                    UserId = userId,
                    UserName = "testuser"
                }
            };

            _mockLogRepository.Setup(repo => repo.GetUserActivityLogsAsync(
                    userId, activityType, pageNumber, pageSize))
                .ReturnsAsync(logs);

            _mockUserRepository.Setup(repo => repo.GetUsersByIdsAsync(It.Is<List<int>>(ids => ids.Contains(userId))))
                .ReturnsAsync(users);

            var query = new GetUserActivityLogsQuery
            {
                UserId = userId,
                ActivityType = activityType,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(logs.Count));

            var resultList = result.ToList();
            for (int i = 0; i < logs.Count; i++)
            {
                Assert.That(resultList[i].LogId, Is.EqualTo(logs[i].LogId));
                Assert.That(resultList[i].UserId, Is.EqualTo(logs[i].UserId));
                Assert.That(resultList[i].Username, Is.EqualTo("testuser"));
                Assert.That(resultList[i].ActivityType, Is.EqualTo(logs[i].ActivityType));
                Assert.That(resultList[i].ActivityDate, Is.EqualTo(logs[i].ActivityDate));
                Assert.That(resultList[i].Details, Is.EqualTo(logs[i].Details));
                Assert.That(resultList[i].IpAddress, Is.EqualTo(logs[i].IpAddress));
            }

            _mockLogRepository.Verify(repo => repo.GetUserActivityLogsAsync(
                userId, activityType, pageNumber, pageSize), Times.Once);
                
            _mockUserRepository.Verify(repo => repo.GetUsersByIdsAsync(
                It.Is<List<int>>(ids => ids.Contains(userId))), Times.Once);
        }

        [Test]
        public async Task Handle_WhenNoLogsFound_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = 1;
            var activityType = "NONEXISTENT";
            var pageNumber = 1;
            var pageSize = 10;

            _mockLogRepository.Setup(repo => repo.GetUserActivityLogsAsync(
                    userId, activityType, pageNumber, pageSize))
                .ReturnsAsync(new List<UserActivityLog>());

            _mockUserRepository.Setup(repo => repo.GetUsersByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Domain.Entities.User>());

            var query = new GetUserActivityLogsQuery
            {
                UserId = userId,
                ActivityType = activityType,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0));

            _mockLogRepository.Verify(repo => repo.GetUserActivityLogsAsync(
                userId, activityType, pageNumber, pageSize), Times.Once);
                
            _mockUserRepository.Verify(repo => repo.GetUsersByIdsAsync(It.IsAny<List<int>>()), Times.Never);
        }

        [Test]
        public async Task Handle_WithMultipleUsers_ShouldReturnCorrectUsernames()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;
            var pageNumber = 1;
            var pageSize = 10;

            var logs = new List<UserActivityLog>
            {
                new UserActivityLog
                {
                    LogId = 1,
                    UserId = userId1,
                    ActivityType = "LOGIN",
                    ActivityDate = DateTime.UtcNow.AddDays(-1),
                    Details = "User 1 logged in",
                    IpAddress = "192.168.1.1"
                },
                new UserActivityLog
                {
                    LogId = 2,
                    UserId = userId2,
                    ActivityType = "LOGIN",
                    ActivityDate = DateTime.UtcNow.AddDays(-1),
                    Details = "User 2 logged in",
                    IpAddress = "192.168.1.2"
                }
            };

            var users = new List<Domain.Entities.User>
            {
                new Domain.Entities.User
                {
                    UserId = userId1,
                    UserName = "user1"
                },
                new Domain.Entities.User
                {
                    UserId = userId2,
                    UserName = "user2"
                }
            };

            _mockLogRepository.Setup(repo => repo.GetUserActivityLogsAsync(
                    0, string.Empty, pageNumber, pageSize))
                .ReturnsAsync(logs);

            _mockUserRepository.Setup(repo => repo.GetUsersByIdsAsync(
                    It.Is<List<int>>(ids => ids.Contains(userId1) && ids.Contains(userId2))))
                .ReturnsAsync(users);

            var query = new GetUserActivityLogsQuery
            {
                UserId = 0, // 0 means all users
                ActivityType = string.Empty,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(logs.Count));

            var resultList = result.ToList();
            Assert.That(resultList[0].Username, Is.EqualTo("user1"));
            Assert.That(resultList[1].Username, Is.EqualTo("user2"));

            _mockLogRepository.Verify(repo => repo.GetUserActivityLogsAsync(
                0, string.Empty, pageNumber, pageSize), Times.Once);
                
            _mockUserRepository.Verify(repo => repo.GetUsersByIdsAsync(
                It.Is<List<int>>(ids => ids.Contains(userId1) && ids.Contains(userId2))), Times.Once);
        }
    }
} 