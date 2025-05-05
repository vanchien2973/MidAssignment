using System;
using NUnit.Framework;
using LibraryManagementSystem.Application.Services;

namespace LibraryManagementSystem.IntegrationTests.Services
{
    [TestFixture]
    public class PasswordHashServiceTests
    {
        private PasswordHashService _passwordHashService;

        [SetUp]
        public void Setup()
        {
            _passwordHashService = new PasswordHashService();
        }

        [Test]
        public void HashPassword_ValidPassword_ReturnsHashedPassword()
        {
            // Arrange
            string password = "Password123!";

            // Act
            string hashedPassword = _passwordHashService.HashPassword(password);

            // Assert
            Assert.That(hashedPassword, Is.Not.Null);
            Assert.That(hashedPassword, Is.Not.Empty);
            Assert.That(hashedPassword, Is.Not.EqualTo(password));
            Assert.That(hashedPassword.Length, Is.EqualTo(64)); 
        }

        [Test]
        public void HashPassword_EmptyPassword_ReturnsHash()
        {
            // Arrange
            string password = "";

            // Act
            string hashedPassword = _passwordHashService.HashPassword(password);

            // Assert
            Assert.That(hashedPassword, Is.Not.Null);
            Assert.That(hashedPassword, Is.Not.Empty);
            Assert.That(hashedPassword.Length, Is.EqualTo(64));
        }

        [Test]
        public void HashPassword_SamePasswordTwice_ReturnsSameHash()
        {
            // Arrange
            string password = "SecurePassword456!";

            // Act
            string hashedPassword1 = _passwordHashService.HashPassword(password);
            string hashedPassword2 = _passwordHashService.HashPassword(password);

            // Assert
            Assert.That(hashedPassword1, Is.EqualTo(hashedPassword2));
        }

        [Test]
        public void HashPassword_DifferentPasswords_ReturnsDifferentHashes()
        {
            // Arrange
            string password1 = "Password123!";
            string password2 = "Password123";

            // Act
            string hashedPassword1 = _passwordHashService.HashPassword(password1);
            string hashedPassword2 = _passwordHashService.HashPassword(password2);

            // Assert
            Assert.That(hashedPassword1, Is.Not.EqualTo(hashedPassword2));
        }

        [Test]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            string password = "Password123!";
            string hashedPassword = _passwordHashService.HashPassword(password);

            // Act
            bool result = _passwordHashService.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyPassword_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            string correctPassword = "Password123!";
            string incorrectPassword = "Password123";
            string hashedPassword = _passwordHashService.HashPassword(correctPassword);

            // Act
            bool result = _passwordHashService.VerifyPassword(incorrectPassword, hashedPassword);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyPassword_CaseSensitivity_ReturnsFalse()
        {
            // Arrange
            string correctPassword = "Password123!";
            string wrongCasePassword = "password123!";
            string hashedPassword = _passwordHashService.HashPassword(correctPassword);

            // Act
            bool result = _passwordHashService.VerifyPassword(wrongCasePassword, hashedPassword);

            // Assert
            Assert.That(result, Is.False);
        }
    }
} 