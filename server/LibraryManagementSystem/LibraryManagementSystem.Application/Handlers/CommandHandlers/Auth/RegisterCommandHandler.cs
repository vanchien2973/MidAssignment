using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using FluentValidation.Results;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Auth
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<RegisterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                // Check if username already exists
                if (await _unitOfWork.Users.UsernameExistsAsync(request.Username))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    var errors = new List<ValidationFailure>
                    {
                        new ValidationFailure("Username", "Username already exists")
                    };
                    throw new ValidationException(errors);
                }
                
                // Check if email already exists
                if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    var errors = new List<ValidationFailure>
                    {
                        new ValidationFailure("Email", "Email already exists")
                    };
                    throw new ValidationException(errors);
                }
                
                // Create new user
                var user = new Domain.Entities.User
                {
                    Username = request.Username,
                    Password = HashPassword(request.Password),
                    Email = request.Email,
                    FullName = request.FullName,
                    UserType = UserType.NormalUser,
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow
                };
                
                await _unitOfWork.Users.CreateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                
                await LogUserActivityAsync(user.UserId, "Registration", "User registered successfully");
                
                await _unitOfWork.CommitTransactionAsync();
                
                return new RegisterResponse { Success = true, Message = "Registration successful" };
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
                
                try
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error rolling back transaction");
                }
                
                return new RegisterResponse { Success = false, Message = "An error occurred during registration" };
            }
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
                var log = new UserActivityLog
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