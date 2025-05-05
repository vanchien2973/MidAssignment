using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Auth
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ILogger<LoginCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Invalid login attempt for non-existent or inactive username: {Username}", request.Username);
                    await _unitOfWork.RollbackTransactionAsync();
                    return new LoginResponse { Success = false };
                }

                var passwordValid = ValidatePassword(request.Password, user.Password);

                if (!passwordValid)
                {
                    _logger.LogWarning("Invalid password for username: {Username}", request.Username);
                    await _unitOfWork.RollbackTransactionAsync();
                    return new LoginResponse { Success = false };
                }

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Log the login
                await LogUserActivityAsync(user.UserId, "Login", "User logged in successfully");
                
                await _unitOfWork.CommitTransactionAsync();

                // Generate tokens
                var token = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
                
                try
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error rolling back transaction");
                }
                
                return new LoginResponse { Success = false };
            }
        }

        private bool ValidatePassword(string inputPassword, string storedPassword)
        {
            var hashedInput = HashPassword(inputPassword);
            return hashedInput == storedPassword;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hash;
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