using System;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Domain.Entities
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void User_InitializeWithValidProperties_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userName = "testuser";
            var passwordHash = "hashedpassword";
            var email = "test@example.com";
            var fullName = "Test User";
            var role = UserRole.Member;
            var isActive = true;
            var registrationDate = DateTime.UtcNow;

            // Act
            var user = new User
            {
                UserId = userId,
                UserName = userName,
                PasswordHash = passwordHash,
                Email = email,
                FullName = fullName,
                Role = role,
                IsActive = isActive,
                RegistrationDate = registrationDate
            };

            // Assert
            Assert.That(user.UserId, Is.EqualTo(userId));
            Assert.That(user.UserName, Is.EqualTo(userName));
            Assert.That(user.PasswordHash, Is.EqualTo(passwordHash));
            Assert.That(user.Email, Is.EqualTo(email));
            Assert.That(user.FullName, Is.EqualTo(fullName));
            Assert.That(user.Role, Is.EqualTo(role));
            Assert.That(user.IsActive, Is.EqualTo(isActive));
            Assert.That(user.RegistrationDate, Is.EqualTo(registrationDate));
        }

        [Test]
        public void User_NavigationProperties_ShouldBeInitializedToNull()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.That(user.BorrowingRequests, Is.Null);
            Assert.That(user.ActivityLogs, Is.Null);
        }

        [Test]
        public void User_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.That(user.UserId, Is.EqualTo(Guid.Empty));
            Assert.That(user.UserName, Is.Null);
            Assert.That(user.PasswordHash, Is.Null);
            Assert.That(user.Email, Is.Null);
            Assert.That(user.FullName, Is.Null);
            Assert.That(user.Role, Is.EqualTo(UserRole.Member)); // Default enum value
            Assert.That(user.IsActive, Is.False);
        }

        [Test]
        public void User_SetRole_ShouldUpdateRoleCorrectly()
        {
            // Arrange
            var user = new User();

            // Act & Assert
            user.Role = UserRole.Member;
            Assert.That(user.Role, Is.EqualTo(UserRole.Member));

            user.Role = UserRole.Librarian;
            Assert.That(user.Role, Is.EqualTo(UserRole.Librarian));

            user.Role = UserRole.Admin;
            Assert.That(user.Role, Is.EqualTo(UserRole.Admin));
        }
    }
} 