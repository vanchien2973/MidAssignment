using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.UnitTests.Domain.Entities
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void User_PropertiesInitialization_PropertiesHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var user = new User();
            
            // Assert
            Assert.That(user.UserId, Is.EqualTo(0));
            Assert.That(user.Username, Is.Null);
            Assert.That(user.Password, Is.Null);
            Assert.That(user.Email, Is.Null);
            Assert.That(user.FullName, Is.Null);
            Assert.That(user.IsActive, Is.EqualTo(false));
            Assert.That(user.UserType, Is.EqualTo(default(UserType)));
            Assert.That(user.CreatedDate, Is.EqualTo(default(DateTime)));
            Assert.That(user.LastLoginDate, Is.Null);
        }
        
        [Test]
        public void User_SetProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var user = new User();
            var userId = 1;
            var username = "johndoe";
            var password = "hashedpassword123";
            var email = "john.doe@example.com";
            var fullName = "John Doe";
            var isActive = true;
            var userType = UserType.SuperUser;
            var createdDate = DateTime.UtcNow;
            var lastLoginDate = DateTime.UtcNow.AddHours(-1);
            
            // Act
            user.UserId = userId;
            user.Username = username;
            user.Password = password;
            user.Email = email;
            user.FullName = fullName;
            user.IsActive = isActive;
            user.UserType = userType;
            user.CreatedDate = createdDate;
            user.LastLoginDate = lastLoginDate;
            
            // Assert
            Assert.That(user.UserId, Is.EqualTo(userId));
            Assert.That(user.Username, Is.EqualTo(username));
            Assert.That(user.Password, Is.EqualTo(password));
            Assert.That(user.Email, Is.EqualTo(email));
            Assert.That(user.FullName, Is.EqualTo(fullName));
            Assert.That(user.IsActive, Is.EqualTo(isActive));
            Assert.That(user.UserType, Is.EqualTo(userType));
            Assert.That(user.CreatedDate, Is.EqualTo(createdDate).Within(TimeSpan.FromSeconds(1)));
            Assert.That(user.LastLoginDate, Is.EqualTo(lastLoginDate).Within(TimeSpan.FromSeconds(1)));
        }
        
        [Test]
        public void User_NavigationProperties_InitializedAsNull()
        {
            // Arrange & Act
            var user = new User();
            
            // Assert
            Assert.That(user.BorrowingRequests, Is.Null);
            Assert.That(user.ApprovedRequests, Is.Null);
            Assert.That(user.ActivityLogs, Is.Null);
        }
        
        [Test]
        public void User_SetNavigationProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var user = new User();
            var borrowingRequests = new List<BookBorrowingRequest>
            {
                new BookBorrowingRequest { RequestId = Guid.NewGuid() },
                new BookBorrowingRequest { RequestId = Guid.NewGuid() }
            };
            var approvedRequests = new List<BookBorrowingRequest>
            {
                new BookBorrowingRequest { RequestId = Guid.NewGuid() }
            };
            var activityLogs = new List<UserActivityLog>
            {
                new UserActivityLog { LogId = 1, ActivityType = "Login" },
                new UserActivityLog { LogId = 2, ActivityType = "View Book" },
                new UserActivityLog { LogId = 3, ActivityType = "Borrow Book" }
            };
            
            // Act
            user.BorrowingRequests = borrowingRequests;
            user.ApprovedRequests = approvedRequests;
            user.ActivityLogs = activityLogs;
            
            // Assert
            Assert.That(user.BorrowingRequests, Is.SameAs(borrowingRequests));
            Assert.That(user.BorrowingRequests.Count, Is.EqualTo(2));
            
            Assert.That(user.ApprovedRequests, Is.SameAs(approvedRequests));
            Assert.That(user.ApprovedRequests.Count, Is.EqualTo(1));
            
            Assert.That(user.ActivityLogs, Is.SameAs(activityLogs));
            Assert.That(user.ActivityLogs.Count, Is.EqualTo(3));
        }
        
        [Test]
        public void User_UserType_EnumValuesAreCorrect()
        {
            // Arrange & Act
            var normalUser = UserType.NormalUser;
            var superUser = UserType.SuperUser;
            
            // Assert
            Assert.That((int)normalUser, Is.EqualTo(1));
            Assert.That((int)superUser, Is.EqualTo(2));
        }
        
        [Test]
        public void User_UserTypeChanges_UserTypeChangesCorrectly()
        {
            // Arrange
            var user = new User
            {
                UserType = UserType.NormalUser
            };
            
            // Act & Assert
            Assert.That(user.UserType, Is.EqualTo(UserType.NormalUser));
            
            user.UserType = UserType.SuperUser;
            Assert.That(user.UserType, Is.EqualTo(UserType.SuperUser));
        }
    }
}
