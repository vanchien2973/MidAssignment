using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Application.Services;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests.Services
{
    [TestFixture]
    public class PasswordHashServiceTests
    {
        private IPasswordHashService _passwordHashService;

        [SetUp]
        public void Setup()
        {
            _passwordHashService = new PasswordHashService();
        }

        [Test]
        public void HashPassword_ReturnsHashedString()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hashedPassword = _passwordHashService.HashPassword(password);

            // Assert
            Assert.That(hashedPassword, Is.Not.Null);
            Assert.That(hashedPassword, Is.Not.Empty);
            Assert.That(hashedPassword, Is.Not.EqualTo(password)); // Hash should not be the original password
            Assert.That(hashedPassword.Length, Is.EqualTo(64)); // SHA-256 produces a 64-character hex string
        }

        [Test]
        public void HashPassword_WithSameInput_ReturnsSameHash()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hashedPassword1 = _passwordHashService.HashPassword(password);
            var hashedPassword2 = _passwordHashService.HashPassword(password);

            // Assert
            Assert.That(hashedPassword1, Is.EqualTo(hashedPassword2));
        }

        [Test]
        public void HashPassword_WithDifferentInputs_ReturnsDifferentHashes()
        {
            // Arrange
            var password1 = "TestPassword123";
            var password2 = "TestPassword124"; // Just one character different

            // Act
            var hashedPassword1 = _passwordHashService.HashPassword(password1);
            var hashedPassword2 = _passwordHashService.HashPassword(password2);

            // Assert
            Assert.That(hashedPassword1, Is.Not.EqualTo(hashedPassword2));
        }

        [Test]
        public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "TestPassword123";
            var hashedPassword = _passwordHashService.HashPassword(password);

            // Act
            var result = _passwordHashService.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var password = "TestPassword123";
            var incorrectPassword = "TestPassword124";
            var hashedPassword = _passwordHashService.HashPassword(password);

            // Act
            var result = _passwordHashService.VerifyPassword(incorrectPassword, hashedPassword);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyPassword_CaseInsensitiveHashComparison()
        {
            // Arrange
            var password = "TestPassword123";
            var hashedPassword = _passwordHashService.HashPassword(password);
            var uppercaseHash = hashedPassword.ToUpper();

            // Act
            var result = _passwordHashService.VerifyPassword(password, uppercaseHash);

            // Assert
            Assert.That(result, Is.True, "Hash comparison should be case-insensitive");
        }

        [Test]
        public void HashPassword_WithEmptyString_ReturnsValidHash()
        {
            // Arrange
            var password = string.Empty;

            // Act
            var hashedPassword = _passwordHashService.HashPassword(password);

            // Assert
            Assert.That(hashedPassword, Is.Not.Null);
            Assert.That(hashedPassword, Is.Not.Empty);
            Assert.That(hashedPassword.Length, Is.EqualTo(64));
        }

        [Test]
        public void HashPassword_WithNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => _passwordHashService.HashPassword(null));
        }
    }
}
