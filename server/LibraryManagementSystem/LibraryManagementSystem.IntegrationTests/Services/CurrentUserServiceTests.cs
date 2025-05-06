using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Application.Services.Implementation;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace LibraryManagementSystem.IntegrationTests.Services
{
    [TestFixture]
    public class CurrentUserServiceTests
    {
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private ICurrentUserService _currentUserService;
        private HttpContext _httpContext;
        private ClaimsIdentity _identity;
        private ClaimsPrincipal _user;

        [SetUp]
        public void Setup()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);
            _currentUserService = new CurrentUserService(_mockHttpContextAccessor.Object);
        }

        private void SetupAuthenticatedUser(int userId, string username, string role)
        {
            _identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            }, "TestAuthentication");

            _user = new ClaimsPrincipal(_identity);
            _httpContext.User = _user;
        }

        [Test]
        public void GetUserId_WhenUserIsAuthenticated_ReturnsUserId()
        {
            // Arrange
            SetupAuthenticatedUser(123, "testuser", "Member");

            // Act
            var userId = _currentUserService.UserId;

            // Assert
            Assert.That(userId, Is.EqualTo(123));
        }

        [Test]
        public void GetUserId_WhenUserIsNotAuthenticated_ReturnsZero()
        {
            // Arrange
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var userId = _currentUserService.UserId;

            // Assert
            Assert.That(userId, Is.EqualTo(0));
        }

        [Test]
        public void GetUsername_WhenUserIsAuthenticated_ReturnsUsername()
        {
            // Arrange
            SetupAuthenticatedUser(123, "testuser", "Member");

            // Act
            var username = _currentUserService.Username;

            // Assert
            Assert.That(username, Is.EqualTo("testuser"));
        }

        [Test]
        public void GetUsername_WhenUserIsNotAuthenticated_ReturnsNull()
        {
            // Arrange
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var username = _currentUserService.Username;

            // Assert
            Assert.That(username, Is.Null);
        }

        [Test]
        public void IsAuthenticated_WhenUserIsAuthenticated_ReturnsTrue()
        {
            // Arrange
            SetupAuthenticatedUser(123, "testuser", "Member");

            // Act
            var isAuthenticated = _currentUserService.IsAuthenticated;

            // Assert
            Assert.That(isAuthenticated, Is.True);
        }

        [Test]
        public void IsAuthenticated_WhenUserIsNotAuthenticated_ReturnsFalse()
        {
            // Arrange
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var isAuthenticated = _currentUserService.IsAuthenticated;

            // Assert
            Assert.That(isAuthenticated, Is.False);
        }

        [Test]
        public void IsSuperUser_WhenUserIsSuperUser_ReturnsTrue()
        {
            // Arrange
            SetupAuthenticatedUser(123, "admin", "SuperUser");

            // Act
            var isSuperUser = _currentUserService.IsSuperUser;

            // Assert
            Assert.That(isSuperUser, Is.True);
        }

        [Test]
        public void IsSuperUser_WhenUserIsNotSuperUser_ReturnsFalse()
        {
            // Arrange
            SetupAuthenticatedUser(123, "testuser", "Member");

            // Act
            var isSuperUser = _currentUserService.IsSuperUser;

            // Assert
            Assert.That(isSuperUser, Is.False);
        }

        [Test]
        public void GetUser_WhenHttpContextExists_ReturnsUserPrincipal()
        {
            // Arrange
            SetupAuthenticatedUser(123, "testuser", "Member");

            // Act
            var user = _currentUserService.User;

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user, Is.EqualTo(_user));
        }

        [Test]
        public void GetUser_WhenHttpContextIsNull_ReturnsNull()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

            // Act
            var user = _currentUserService.User;

            // Assert
            Assert.That(user, Is.Null);
        }
    }
}
