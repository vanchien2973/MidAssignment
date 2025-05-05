using System.Security.Claims;
using LibraryManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace LibraryManagementSystem.Application.Services.Implementation;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId => GetUserId();
    
    public string Username => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public bool IsSuperUser => _httpContextAccessor.HttpContext?.User?.IsInRole("SuperUser") ?? false;
    
    public ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;

    private int GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        
        return 0; // Default value indicating not authenticated or no userId claim
    }
} 