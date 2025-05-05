using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Auth
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var principal = _tokenService.GetPrincipalFromExpiredToken(request.Token);
                var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new RefreshTokenResponse { Success = false };
                }
                
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                
                if (user == null || !user.IsActive)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new RefreshTokenResponse { Success = false };
                }
                
                // Log the token refresh activity
                await LogUserActivityAsync(user.UserId, "TokenRefresh", "User refreshed token");
                
                await _unitOfWork.CommitTransactionAsync();
                
                // Generate new tokens
                var newToken = _tokenService.GenerateAccessToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                
                return new RefreshTokenResponse
                {
                    Success = true,
                    Token = newToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                
                try
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error rolling back transaction");
                }
                
                return new RefreshTokenResponse { Success = false };
            }
        }
        
        private async Task LogUserActivityAsync(int userId, string activityType, string details)
        {
            try
            {
                var log = new Domain.Entities.UserActivityLog
                {
                    UserId = userId,
                    ActivityType = activityType,
                    ActivityDate = DateTime.UtcNow,
                    Details = details,
                    IpAddress = "::1" // Local IP for demo
                };
                
                await _unitOfWork.UserActivityLogs.LogActivityAsync(log);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user activity for user {UserId}", userId);
            }
        }
    }
} 