using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.User;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Queries.User
{
    [TestFixture]
    public class GetAllUsersQueryHandlerTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private GetAllUsersQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _handler = new GetAllUsersQueryHandler(_mockUserRepository.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnPaginatedUsers()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var searchTerm = "test";
            var totalCount = 25;

            var users = new List<Domain.Entities.User>
            {
                new Domain.Entities.User
                {
                    UserId = 1,
                    UserName = "testuser1",
                    Email = "test1@example.com",
                    FullName = "Test User 1",
                    Role = UserRole.Member,
                    IsActive = true
                },
                new Domain.Entities.User
                {
                    UserId = 2,
                    UserName = "testuser2",
                    Email = "test2@example.com",
                    FullName = "Test User 2",
                    Role = UserRole.Librarian,
                    IsActive = true
                }
            };

            _mockUserRepository.Setup(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm))
                .ReturnsAsync(users);

            _mockUserRepository.Setup(repo => repo.CountBySearchTermAsync(searchTerm))
                .ReturnsAsync(totalCount);

            var query = new GetAllUsersQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data.Count(), Is.EqualTo(users.Count));
            Assert.That(result.TotalCount, Is.EqualTo(totalCount));
            Assert.That(result.PageNumber, Is.EqualTo(pageNumber));
            Assert.That(result.PageSize, Is.EqualTo(pageSize));

            // Verify that the mapping was done correctly
            var resultList = result.Data.ToList();
            for (int i = 0; i < users.Count; i++)
            {
                Assert.That(resultList[i].UserId, Is.EqualTo(users[i].UserId));
                Assert.That(resultList[i].UserName, Is.EqualTo(users[i].UserName));
                Assert.That(resultList[i].Email, Is.EqualTo(users[i].Email));
                Assert.That(resultList[i].FullName, Is.EqualTo(users[i].FullName));
            }

            _mockUserRepository.Verify(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm), Times.Once);
            _mockUserRepository.Verify(repo => repo.CountBySearchTermAsync(searchTerm), Times.Once);
        }

        [Test]
        public async Task Handle_WhenNoUsersFound_ShouldReturnEmptyList()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var searchTerm = "nonexistent";
            var totalCount = 0;

            _mockUserRepository.Setup(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm))
                .ReturnsAsync(new List<Domain.Entities.User>());

            _mockUserRepository.Setup(repo => repo.CountBySearchTermAsync(searchTerm))
                .ReturnsAsync(totalCount);

            var query = new GetAllUsersQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data.Count(), Is.EqualTo(0));
            Assert.That(result.TotalCount, Is.EqualTo(totalCount));

            _mockUserRepository.Verify(repo => repo.GetUsersAsync(pageNumber, pageSize, searchTerm), Times.Once);
            _mockUserRepository.Verify(repo => repo.CountBySearchTermAsync(searchTerm), Times.Once);
        }
    }
} 