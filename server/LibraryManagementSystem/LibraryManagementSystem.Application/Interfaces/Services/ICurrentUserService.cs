using System.Security.Claims;

namespace LibraryManagementSystem.Application.Interfaces.Services;

public interface ICurrentUserService
{
    int UserId { get; }
    bool IsAuthenticated { get; }
} 