using System;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.UnitTests.Domain.Entities
{
    [TestFixture]
    public class UserActivityLogTests
    {
        [Test]
        public void UserActivityLog_PropertiesInitialization_PropertiesHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var userActivityLog = new UserActivityLog();
            
            // Assert
            Assert.That(userActivityLog.LogId, Is.EqualTo(0));
            Assert.That(userActivityLog.UserId, Is.EqualTo(0));
            Assert.That(userActivityLog.ActivityType, Is.Null);
            Assert.That(userActivityLog.ActivityDate, Is.EqualTo(default(DateTime)));
            Assert.That(userActivityLog.Details, Is.Null);
            Assert.That(userActivityLog.IpAddress, Is.Null);
            Assert.That(userActivityLog.User, Is.Null);
        }
        
        [Test]
        public void UserActivityLog_SetProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var userActivityLog = new UserActivityLog();
            var logId = 1;
            var userId = 2;
            var activityType = "Login";
            var activityDate = DateTime.UtcNow;
            var details = "User logged in successfully";
            var ipAddress = "192.168.1.1";
            
            // Act
            userActivityLog.LogId = logId;
            userActivityLog.UserId = userId;
            userActivityLog.ActivityType = activityType;
            userActivityLog.ActivityDate = activityDate;
            userActivityLog.Details = details;
            userActivityLog.IpAddress = ipAddress;
            
            // Assert
            Assert.That(userActivityLog.LogId, Is.EqualTo(logId));
            Assert.That(userActivityLog.UserId, Is.EqualTo(userId));
            Assert.That(userActivityLog.ActivityType, Is.EqualTo(activityType));
            Assert.That(userActivityLog.ActivityDate, Is.EqualTo(activityDate));
            Assert.That(userActivityLog.Details, Is.EqualTo(details));
            Assert.That(userActivityLog.IpAddress, Is.EqualTo(ipAddress));
        }
        
        [Test]
        public void UserActivityLog_NavigationProperties_InitializedAsNull()
        {
            // Arrange & Act
            var userActivityLog = new UserActivityLog();
            
            // Assert
            Assert.That(userActivityLog.User, Is.Null);
        }
        
        [Test]
        public void UserActivityLog_SetNavigationProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var userActivityLog = new UserActivityLog();
            var user = new User 
            { 
                UserId = 1, 
                Username = "johndoe", 
                Email = "john.doe@example.com" 
            };
            
            // Act
            userActivityLog.User = user;
            
            // Assert
            Assert.That(userActivityLog.User, Is.SameAs(user));
            Assert.That(userActivityLog.User.Username, Is.EqualTo("johndoe"));
        }
    }
} 