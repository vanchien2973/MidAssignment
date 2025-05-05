using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests.Repositories
{
    public class UserRepositoryTests : TestBase
    {
        private UserRepository _userRepository;

        public override async Task Setup()
        {
            await base.Setup();
            _userRepository = new UserRepository(DbContext);
        }

        protected override async Task SeedDataAsync()
        {
            // Create test users
            var users = new[]
            {
                new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = "admin",
                    PasswordHash = "hashed_password_1",
                    Email = "admin@example.com",
                    FullName = "Admin User",
                    Role = UserRole.Admin,
                    IsActive = true
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = "librarian",
                    PasswordHash = "hashed_password_2",
                    Email = "librarian@example.com",
                    FullName = "Librarian User",
                    Role = UserRole.Librarian,
                    IsActive = true
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = "member",
                    PasswordHash = "hashed_password_3",
                    Email = "member@example.com",
                    FullName = "Member User",
                    Role = UserRole.Member,
                    IsActive = true
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = "inactive",
                    PasswordHash = "hashed_password_4",
                    Email = "inactive@example.com",
                    FullName = "Inactive User",
                    Role = UserRole.Member,
                    IsActive = false
                }
            };
            
            await DbContext.Users.AddRangeAsync(users);
            await DbContext.SaveChangesAsync();
        }

        [Test]
        public async Task GetByIdAsync_ExistingUser_ReturnsCorrectUser()
        {
            // Arrange
            var existingUser = await DbContext.Users.FirstAsync();
            
            // Act
            var user = await _userRepository.GetByIdAsync(existingUser.UserId);
            
            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.UserId, Is.EqualTo(existingUser.UserId));
            Assert.That(user.UserName, Is.EqualTo(existingUser.UserName));
        }

        [Test]
        public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            // Act
            var user = await _userRepository.GetByIdAsync(nonExistingId);
            
            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_DefaultParameters_ReturnsPaginatedResults()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            
            // Act
            var users = (await _userRepository.GetAllAsync(pageNumber, pageSize)).ToList();
            
            // Assert
            Assert.That(users, Is.Not.Null);
            Assert.That(users.Count, Is.EqualTo(pageSize));
        }

        [Test]
        public async Task GetAllAsync_WithSorting_ReturnsSortedResults()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            
            // Act - Sort by username ascending
            var usersAsc = (await _userRepository.GetAllAsync(pageNumber, pageSize, "username", "asc")).ToList();
            
            // Assert
            Assert.That(usersAsc, Is.Not.Null);
            Assert.That(usersAsc, Is.Ordered.By("UserName"));
            
            // Act - Sort by role descending
            var usersDesc = (await _userRepository.GetAllAsync(pageNumber, pageSize, "role", "desc")).ToList();
            
            // Assert
            Assert.That(usersDesc, Is.Not.Null);
            Assert.That(usersDesc, Is.Ordered.Descending.By("Role"));
        }

        [Test]
        public async Task GetByUsernameAsync_ExistingUsername_ReturnsCorrectUser()
        {
            // Act
            var user = await _userRepository.GetByUsernameAsync("admin");
            
            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.UserName, Is.EqualTo("admin"));
        }

        [Test]
        public async Task GetByUsernameAsync_NonExistingUsername_ReturnsNull()
        {
            // Act
            var user = await _userRepository.GetByUsernameAsync("non-existing-user");
            
            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsCorrectUser()
        {
            // Act
            var user = await _userRepository.GetByEmailAsync("admin@example.com");
            
            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Email, Is.EqualTo("admin@example.com"));
        }

        [Test]
        public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
        {
            // Act
            var user = await _userRepository.GetByEmailAsync("non-existing@example.com");
            
            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public async Task GetActiveUsersByRoleAsync_ExistingRole_ReturnsCorrectUsers()
        {
            // Act
            var users = (await _userRepository.GetActiveUsersByRoleAsync(UserRole.Member, 1, 10)).ToList();
            
            // Assert
            Assert.That(users, Is.Not.Null);
            Assert.That(users.Count, Is.EqualTo(1)); // Only one active member
            Assert.That(users.All(u => u.Role == UserRole.Member && u.IsActive), Is.True);
        }

        [Test]
        public async Task GetActiveUsersByRoleAsync_NonExistingRole_ReturnsEmptyList()
        {
            // Use a role value that doesn't exist in the seeded data
            var nonExistingRole = (UserRole)999;
            
            // Act
            var users = (await _userRepository.GetActiveUsersByRoleAsync(nonExistingRole, 1, 10)).ToList();
            
            // Assert
            Assert.That(users, Is.Empty);
        }

        [Test]
        public async Task CreateAsync_ValidUser_AddsUserToContext()
        {
            // Arrange
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                UserName = "newuser",
                PasswordHash = "hashed_password_new",
                Email = "newuser@example.com",
                FullName = "New User",
                Role = UserRole.Member,
                IsActive = true
            };
            
            // Act
            var result = await _userRepository.CreateAsync(newUser);
            await DbContext.SaveChangesAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(newUser.UserId));
            
            // Verify user was added to context
            var addedUser = await DbContext.Users.FindAsync(newUser.UserId);
            Assert.That(addedUser, Is.Not.Null);
            Assert.That(addedUser.UserName, Is.EqualTo("newuser"));
        }

        [Test]
        public async Task UpdateAsync_ExistingUser_UpdatesUserInContext()
        {
            // Arrange
            var existingUser = await DbContext.Users.FirstAsync();
            existingUser.FullName = "Updated Full Name";
            existingUser.Email = "updated@example.com";
            
            // Act
            await _userRepository.UpdateAsync(existingUser);
            await DbContext.SaveChangesAsync();
            
            // Assert - Reload from database to verify changes
            DbContext.Entry(existingUser).Reload();
            Assert.That(existingUser.FullName, Is.EqualTo("Updated Full Name"));
            Assert.That(existingUser.Email, Is.EqualTo("updated@example.com"));
        }

        [Test]
        public async Task DeleteAsync_ExistingUser_RemovesUserFromContext()
        {
            // Arrange
            var existingUser = await DbContext.Users.FirstAsync();
            var userId = existingUser.UserId;
            
            // Act
            await _userRepository.DeleteAsync(userId);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var deletedUser = await DbContext.Users.FindAsync(userId);
            Assert.That(deletedUser, Is.Null);
        }

        [Test]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            // Act
            var count = await _userRepository.CountAsync();
            
            // Assert
            Assert.That(count, Is.EqualTo(4));
        }

        [Test]
        public async Task GetUserCountByRoleAsync_ExistingRole_ReturnsCorrectCount()
        {
            // Act
            var count = await _userRepository.GetUserCountByRoleAsync(UserRole.Member);
            
            // Assert
            Assert.That(count, Is.EqualTo(2)); // There are 2 members (one active, one inactive)
        }

        [Test]
        public async Task UsernameExistsAsync_ExistingUsername_ReturnsTrue()
        {
            // Act
            var exists = await _userRepository.UsernameExistsAsync("admin");
            
            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task UsernameExistsAsync_NonExistingUsername_ReturnsFalse()
        {
            // Act
            var exists = await _userRepository.UsernameExistsAsync("non-existing-user");
            
            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
        {
            // Act
            var exists = await _userRepository.EmailExistsAsync("admin@example.com");
            
            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task EmailExistsAsync_NonExistingEmail_ReturnsFalse()
        {
            // Act
            var exists = await _userRepository.EmailExistsAsync("non-existing@example.com");
            
            // Assert
            Assert.That(exists, Is.False);
        }
    }
} 