using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Context;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private DbContextOptions<LibraryDbContext> _options;
        private LibraryDbContext _context;
        private UserRepository _repository;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new LibraryDbContext(_options);
            _repository = new UserRepository(_context);

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
            var users = new List<User>
            {
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    Password = "hashed_password_admin",
                    Email = "admin@example.com",
                    FullName = "Admin User",
                    IsActive = true,
                    UserType = UserType.SuperUser,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new User
                {
                    UserId = 2,
                    Username = "librarian",
                    Password = "hashed_password_librarian",
                    Email = "librarian@example.com",
                    FullName = "Librarian User",
                    IsActive = true,
                    UserType = UserType.SuperUser,
                    CreatedDate = DateTime.UtcNow.AddMonths(-3)
                },
                new User
                {
                    UserId = 3,
                    Username = "member",
                    Password = "hashed_password_member",
                    Email = "member@example.com",
                    FullName = "Member User",
                    IsActive = true,
                    UserType = UserType.NormalUser,
                    CreatedDate = DateTime.UtcNow.AddMonths(-1)
                },
                new User
                {
                    UserId = 4,
                    Username = "inactive",
                    Password = "hashed_password_inactive",
                    Email = "inactive@example.com",
                    FullName = "Inactive User",
                    IsActive = false,
                    UserType = UserType.NormalUser,
                    CreatedDate = DateTime.UtcNow.AddMonths(-2)
                }
            };

            _context.Users.AddRange(users);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetByIdAsync_ExistingUserId_ReturnsUser()
        {
            // Arrange
            int userId = 1;

            // Act
            var result = await _repository.GetByIdAsync(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Username, Is.EqualTo("admin"));
        }

        [Test]
        public async Task GetByIdAsync_NonExistentUserId_ReturnsNull()
        {
            // Arrange
            int nonExistentUserId = 999;

            // Act
            var result = await _repository.GetByIdAsync(nonExistentUserId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByUsernameAsync_ExistingUsername_ReturnsUser()
        {
            // Arrange
            string username = "admin";

            // Act
            var result = await _repository.GetByUsernameAsync(username);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo(username));
            Assert.That(result.UserId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetByUsernameAsync_NonExistentUsername_ReturnsNull()
        {
            // Arrange
            string nonExistentUsername = "nonexistent";

            // Act
            var result = await _repository.GetByUsernameAsync(nonExistentUsername);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
        {
            // Arrange
            string email = "admin@example.com";

            // Act
            var result = await _repository.GetByEmailAsync(email);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(email));
            Assert.That(result.UserId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetByEmailAsync_NonExistentEmail_ReturnsNull()
        {
            // Arrange
            string nonExistentEmail = "nonexistent@example.com";

            // Act
            var result = await _repository.GetByEmailAsync(nonExistentEmail);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetUsersAsync_ReturnsAllUsers()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _repository.GetUsersAsync(pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(4));
        }

        [Test]
        public async Task GetUsersAsync_WithSearchTerm_ReturnsFilteredUsers()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            string searchTerm = "admin";

            // Act
            var result = await _repository.GetUsersAsync(pageNumber, pageSize, searchTerm);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Username, Is.EqualTo("admin"));
        }

        [Test]
        public async Task GetUsersAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            int pageNumber = 2;
            int pageSize = 2;

            // Act
            var result = await _repository.GetUsersAsync(pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            var userIds = result.Select(u => u.UserId).ToList();
            Assert.That(userIds, Contains.Item(3));
            Assert.That(userIds, Contains.Item(4));
        }

        [Test]
        public async Task GetUsersByIdsAsync_ReturnsUsersWithMatchingIds()
        {
            // Arrange
            var userIds = new List<int> { 1, 3 };

            // Act
            var result = await _repository.GetUsersByIdsAsync(userIds);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            foreach (var user in result)
            {
                Assert.That(userIds, Contains.Item(user.UserId));
            }
        }

        [Test]
        public async Task UsernameExistsAsync_WithExistingUsername_ReturnsTrue()
        {
            // Arrange
            string existingUsername = "admin";

            // Act
            var result = await _repository.UsernameExistsAsync(existingUsername);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task UsernameExistsAsync_WithNonExistentUsername_ReturnsFalse()
        {
            // Arrange
            string nonExistentUsername = "nonexistent";

            // Act
            var result = await _repository.UsernameExistsAsync(nonExistentUsername);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task EmailExistsAsync_WithExistingEmail_ReturnsTrue()
        {
            // Arrange
            string existingEmail = "admin@example.com";

            // Act
            var result = await _repository.EmailExistsAsync(existingEmail);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task EmailExistsAsync_WithNonExistentEmail_ReturnsFalse()
        {
            // Arrange
            string nonExistentEmail = "nonexistent@example.com";

            // Act
            var result = await _repository.EmailExistsAsync(nonExistentEmail);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task EmailExistsAsync_WithExistingEmailButExceptUserId_ReturnsFalse()
        {
            // Arrange
            string existingEmail = "admin@example.com";
            int userId = 1;

            // Act
            var result = await _repository.EmailExistsAsync(existingEmail, userId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CreateAsync_ValidUser_AddsToDatabase()
        {
            // Arrange
            var newUser = new User
            {
                Username = "newuser",
                Password = "hashed_password_new",
                Email = "newuser@example.com",
                FullName = "New User",
                IsActive = true,
                UserType = UserType.NormalUser,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            await _repository.CreateAsync(newUser);
            await _context.SaveChangesAsync();

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            Assert.That(savedUser, Is.Not.Null);
            Assert.That(savedUser.Email, Is.EqualTo("newuser@example.com"));
        }

        [Test]
        public async Task UpdateAsync_ExistingUser_UpdatesInDatabase()
        {
            // Arrange
            var user = await _context.Users.FindAsync(1);
            user.FullName = "Updated Admin Name";
            user.Email = "updated.admin@example.com";

            // Act
            await _repository.UpdateAsync(user);
            await _context.SaveChangesAsync();

            // Assert
            var updatedUser = await _context.Users.FindAsync(1);
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.FullName, Is.EqualTo("Updated Admin Name"));
            Assert.That(updatedUser.Email, Is.EqualTo("updated.admin@example.com"));
        }

        [Test]
        public async Task DeleteAsync_ExistingUser_RemovesFromDatabase()
        {
            // Arrange
            int userId = 4; // inactive user

            // Act
            await _repository.DeleteAsync(userId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedUser = await _context.Users.FindAsync(userId);
            Assert.That(deletedUser, Is.Null);
        }

        [Test]
        public async Task CountBySearchTermAsync_ReturnsCorrectCount()
        {
            // Act
            var result = await _repository.CountBySearchTermAsync();

            // Assert
            Assert.That(result, Is.EqualTo(4));
        }

        [Test]
        public async Task CountBySearchTermAsync_WithSearchTerm_ReturnsFilteredCount()
        {
            // Arrange
            string searchTerm = "member";

            // Act
            var result = await _repository.CountBySearchTermAsync(searchTerm);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }
    }
}
