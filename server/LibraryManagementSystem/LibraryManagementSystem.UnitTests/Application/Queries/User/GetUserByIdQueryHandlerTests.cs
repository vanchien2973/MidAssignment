using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.User;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Queries.User
{
    [TestFixture]
    public class GetUserByIdQueryHandlerTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private GetUserByIdQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _handler = new GetUserByIdQueryHandler(_mockUserRepository.Object);
        }

        [Test]
        public async Task Handle_WhenUserExists_ShouldReturnUserDto()
        {
            // Arrange
            var userId = 1;
            var user = new Domain.Entities.User
            {
                UserId = userId,
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                Role = UserRole.Member,
                IsActive = true,
                RegistrationDate = DateTime.UtcNow.AddDays(-30)
            };
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);
                
            var query = new GetUserByIdQuery(userId);
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.UserName, Is.EqualTo(user.UserName));
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.FullName, Is.EqualTo(user.FullName));
            
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task Handle_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var userId = 999;
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((Domain.Entities.User)null);
                
            var query = new GetUserByIdQuery(userId);
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Null);
            
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }
    }
} 