using System.Security.Claims;

namespace LibraryManagementSystem.Application.Interfaces.Services;

public interface ICurrentUserService
{
    int UserId { get; }
    string Username { get; }
    bool IsAuthenticated { get; }
    bool IsSuperUser { get; }
    ClaimsPrincipal User { get; }
} 