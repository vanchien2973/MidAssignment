using MediatR;

namespace LibraryManagementSystem.Application.Queries.User;

public class GetUserActivityLogsQuery : IRequest<IEnumerable<UserActivityLogDto>>
{
    public int UserId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UserActivityLogDto
{
    public int LogId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string ActivityType { get; set; }
    public DateTime ActivityDate { get; set; }
    public string Details { get; set; }
    public string IpAddress { get; set; }
} 