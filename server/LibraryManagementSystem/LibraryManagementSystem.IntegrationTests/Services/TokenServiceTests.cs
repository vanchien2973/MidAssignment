using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Application.Services.Implementation;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace LibraryManagementSystem.IntegrationTests.Services
{
    [TestFixture]
    public class TokenServiceTests
    {
        private ITokenService _tokenService;
        private Mock<IConfiguration> _mockConfiguration;
        private User _testUser;

        [SetUp]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Configure mock for configuration
            var configValues = new Dictionary<string, string>
            {
                { "Jwt:Key", "ThisIsAVeryLongSecretKeyForTestingTokenGenerationAndValidation" },
                { "Jwt:Issuer", "librarymanagement.test" },
                { "Jwt:Audience", "library.users" },
                { "Jwt:DurationInMinutes", "60" }
            };

            _mockConfiguration.Setup(x => x[It.IsAny<string>()]).Returns<string>(key => configValues.GetValueOrDefault(key));
            
            _tokenService = new TokenService(_mockConfiguration.Object);
            
            _testUser = new User
            {
                UserId = 123,
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                UserType = UserType.NormalUser
            };
        }

        [Test]
        public void GenerateAccessToken_ReturnsValidToken()
        {
            // Act
            var token = _tokenService.GenerateAccessToken(_testUser);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.Not.Empty);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            Assert.That(tokenHandler.CanReadToken(token), Is.True);
        }

        [Test]
        public void GenerateAccessToken_ContainsCorrectClaims()
        {
            // Act
            var token = _tokenService.GenerateAccessToken(_testUser);
            
            // Decode the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Assert
            Assert.That(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, Is.EqualTo(_testUser.UserId.ToString()));
            Assert.That(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value, Is.EqualTo(_testUser.Username));
            Assert.That(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value, Is.EqualTo(_testUser.Email));
            Assert.That(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value, Is.EqualTo(_testUser.UserType.ToString()));
            Assert.That(jwtToken.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value, Is.EqualTo(_testUser.FullName));
        }

        [Test]
        public void GenerateAccessToken_HasCorrectIssuerAndAudience()
        {
            // Act
            var token = _tokenService.GenerateAccessToken(_testUser);
            
            // Decode the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Assert
            Assert.That(jwtToken.Issuer, Is.EqualTo("librarymanagement.test"));
            Assert.That(jwtToken.Audiences.First(), Is.EqualTo("library.users"));
        }

        [Test]
        public void GenerateAccessToken_HasValidExpirationTime()
        {
            // Act
            var token = _tokenService.GenerateAccessToken(_testUser);
            
            // Decode the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Assert
            var now = DateTime.UtcNow;
            Assert.That(jwtToken.ValidTo, Is.GreaterThan(now));
            
            // Token should expire in approximately 60 minutes (with a small margin for execution time)
            var expectedExpiration = now.AddMinutes(60);
            var timeDifference = Math.Abs((jwtToken.ValidTo - expectedExpiration).TotalMinutes);
            Assert.That(timeDifference, Is.LessThan(1)); // Allow 1 minute difference for test execution time
        }

        [Test]
        public void GenerateRefreshToken_ReturnsNonEmptyString()
        {
            // Act
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            // Assert
            Assert.That(refreshToken, Is.Not.Null);
            Assert.That(refreshToken, Is.Not.Empty);
        }

        [Test]
        public void GenerateRefreshToken_ReturnsDifferentTokensOnEachCall()
        {
            // Act
            var refreshToken1 = _tokenService.GenerateRefreshToken();
            var refreshToken2 = _tokenService.GenerateRefreshToken();
            
            // Assert
            Assert.That(refreshToken1, Is.Not.EqualTo(refreshToken2));
        }

        [Test]
        public void GetPrincipalFromExpiredToken_WithValidToken_ReturnsPrincipal()
        {
            // Arrange
            var token = _tokenService.GenerateAccessToken(_testUser);
            
            // Act
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);
            
            // Assert
            Assert.That(principal, Is.Not.Null);
            Assert.That(principal.Identity.IsAuthenticated, Is.True);
            Assert.That(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, Is.EqualTo(_testUser.UserId.ToString()));
            Assert.That(principal.FindFirst(ClaimTypes.Name)?.Value, Is.EqualTo(_testUser.Username));
        }

        [Test]
        public void GetPrincipalFromExpiredToken_WithInvalidToken_ThrowsException()
        {
            // Arrange
            var invalidToken = "invalid.token.string";
            
            // Act & Assert
            Assert.That(() => _tokenService.GetPrincipalFromExpiredToken(invalidToken), 
                Throws.TypeOf<ArgumentException>().Or.TypeOf<SecurityTokenException>());
        }
    }
}
