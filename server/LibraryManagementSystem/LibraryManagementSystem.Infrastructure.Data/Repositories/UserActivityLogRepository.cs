using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories;

public class UserActivityLogRepository : IUserActivityLogRepository
{
    private readonly LibraryDbContext _context;
    
    public UserActivityLogRepository(LibraryDbContext context)
    {
        _context = context;
    }
    
    public async Task<UserActivityLog> LogActivityAsync(UserActivityLog log)
    {
        await _context.UserActivityLogs.AddAsync(log);
        return log;
    }
    
    public async Task<IEnumerable<UserActivityLog>> GetUserActivityLogsAsync(
        int userId, 
        string activityType = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.UserActivityLogs.AsQueryable();
        query = query.Where(l => l.UserId == userId);
        
        if (!string.IsNullOrEmpty(activityType))
        {
            query = query.Where(l => l.ActivityType == activityType);
        }
        
        query = query.OrderByDescending(l => l.ActivityDate);
        
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}