using LibraryManagementSystem.Domain.Entities;
using System.Security.Claims;

namespace LibraryManagementSystem.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}