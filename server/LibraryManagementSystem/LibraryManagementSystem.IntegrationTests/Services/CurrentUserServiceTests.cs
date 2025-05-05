using System.Security.Claims;
using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Application.Services.Implementation;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests.Services
{
    [TestFixture]
    public class CurrentUserServiceTests
    {
        private ICurrentUserService _currentUserService;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private HttpContext _httpContext;
        private ClaimsIdentity _identity;
        private ClaimsPrincipal _user;

        [SetUp]
        public void Setup()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContext = new DefaultHttpContext();
            _identity = new ClaimsIdentity();
            _user = new ClaimsPrincipal(_identity);
            _httpContext.User = _user;
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);
            
            _currentUserService = new CurrentUserService(_httpContextAccessorMock.Object);
        }

        [Test]
        public void GetUserId_WithValidUserIdClaim_ReturnsCorrectId()
        {
            // Arrange
            const int expectedUserId = 42;
            _identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()));
            
            // Act
            int userId = _currentUserService.UserId;
            
            // Assert
            Assert.That(userId, Is.EqualTo(expectedUserId));
        }

        [Test]
        public void GetUserId_WithoutUserIdClaim_ReturnsZero()
        {
            // Act
            int userId = _currentUserService.UserId;
            
            // Assert
            Assert.That(userId, Is.EqualTo(0));
        }

        [Test]
        public void GetUserId_WithNonIntegerUserIdClaim_ReturnsZero()
        {
            // Arrange
            _identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "not-an-integer"));
            
            // Act
            int userId = _currentUserService.UserId;
            
            // Assert
            Assert.That(userId, Is.EqualTo(0));
        }

        [Test]
        public void Username_WithIdentityName_ReturnsCorrectUsername()
        {
            // Arrange
            const string expectedUsername = "testuser";
            _identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, expectedUsername) }, "test");
            _user = new ClaimsPrincipal(_identity);
            _httpContext.User = _user;
            
            // Act
            string username = _currentUserService.Username;
            
            // Assert
            Assert.That(username, Is.EqualTo(expectedUsername));
        }

        [Test]
        public void Username_WithoutIdentityName_ReturnsNull()
        {
            // Act
            string username = _currentUserService.Username;
            
            // Assert
            Assert.That(username, Is.Null);
        }

        [Test]
        public void IsAuthenticated_WithAuthenticatedIdentity_ReturnsTrue()
        {
            // Arrange
            _identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }, "test");
            _user = new ClaimsPrincipal(_identity);
            _httpContext.User = _user;
            
            // Act
            bool isAuthenticated = _currentUserService.IsAuthenticated;
            
            // Assert
            Assert.That(isAuthenticated, Is.True);
        }

        [Test]
        public void IsAuthenticated_WithoutAuthenticatedIdentity_ReturnsFalse()
        {
            // Act
            bool isAuthenticated = _currentUserService.IsAuthenticated;
            
            // Assert
            Assert.That(isAuthenticated, Is.False);
        }

        [Test]
        public void IsSuperUser_WithSuperUserRole_ReturnsTrue()
        {
            // Arrange
            _identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "SuperUser") }, "test");
            _user = new ClaimsPrincipal(_identity);
            _httpContext.User = _user;
            
            // Act
            bool isSuperUser = _currentUserService.IsSuperUser;
            
            // Assert
            Assert.That(isSuperUser, Is.True);
        }

        [Test]
        public void IsSuperUser_WithoutSuperUserRole_ReturnsFalse()
        {
            // Arrange
            _identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "NormalUser") }, "test");
            _user = new ClaimsPrincipal(_identity);
            _httpContext.User = _user;
            
            // Act
            bool isSuperUser = _currentUserService.IsSuperUser;
            
            // Assert
            Assert.That(isSuperUser, Is.False);
        }

        [Test]
        public void User_ReturnsCorrectUser()
        {
            // Act
            var user = _currentUserService.User;
            
            // Assert
            Assert.That(user, Is.SameAs(_user));
        }

        [Test]
        public void User_NullHttpContext_ReturnsNull()
        {
            // Arrange
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
            
            // Act
            var user = _currentUserService.User;
            
            // Assert
            Assert.That(user, Is.Null);
        }
    }
} 