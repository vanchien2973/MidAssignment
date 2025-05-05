using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.Repositories;

public interface IUserActivityLogRepository
{
    Task<UserActivityLog> LogActivityAsync(UserActivityLog log);
    Task<IEnumerable<UserActivityLog>> GetUserActivitiesAsync(int userId, int pageNumber, int pageSize);
    Task<IEnumerable<UserActivityLog>> GetRecentActivitiesAsync(int pageNumber, int pageSize);
    Task<IEnumerable<UserActivityLog>> GetUserActivityLogsAsync(
        int userId, 
        string activityType = null,
        int pageNumber = 1,
        int pageSize = 10);
}