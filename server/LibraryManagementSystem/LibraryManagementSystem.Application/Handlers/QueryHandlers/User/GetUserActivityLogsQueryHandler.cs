using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.User;
using MediatR;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.User;

public class GetUserActivityLogsQueryHandler : IRequestHandler<GetUserActivityLogsQuery, IEnumerable<UserActivityLogDto>>
{
    private readonly IUserActivityLogRepository _logRepository;
    private readonly IUserRepository _userRepository;
    
    public GetUserActivityLogsQueryHandler(
        IUserActivityLogRepository logRepository, 
        IUserRepository userRepository)
    {
        _logRepository = logRepository;
        _userRepository = userRepository;
    }
    
    public async Task<IEnumerable<UserActivityLogDto>> Handle(GetUserActivityLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _logRepository.GetUserActivityLogsAsync(
            request.UserId, 
            request.ActivityType,
            request.PageNumber,
            request.PageSize);
        
        // Get all user information for the logs
        var userIds = logs.Select(l => l.UserId).Distinct().ToList();
        var users = (await _userRepository.GetUsersByIdsAsync(userIds))
                    .ToDictionary(u => u.UserId, u => u.Username);
        
        return logs.ToDtos(users);
    }
} 