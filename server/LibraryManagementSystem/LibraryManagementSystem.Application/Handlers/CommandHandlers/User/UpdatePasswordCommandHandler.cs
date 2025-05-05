using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.User;

public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePasswordCommandHandler> _logger;
    private readonly IPasswordHashService _passwordHashService;
    
    public UpdatePasswordCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdatePasswordCommandHandler> logger,
        IPasswordHashService passwordHashService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHashService = passwordHashService;
    }
    
    public async Task<bool> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {   
            await _unitOfWork.BeginTransactionAsync();
            
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found during password update attempt", request.UserId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
            
            // Verify current password
            if (!_passwordHashService.VerifyPassword(request.CurrentPassword, user.Password))
            {
                _logger.LogWarning("Invalid current password provided for user {UserId}", request.UserId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
            
            // Hash new password
            string newPasswordHash = _passwordHashService.HashPassword(request.NewPassword);
            user.Password = newPasswordHash;
            
            await _unitOfWork.Users.UpdateAsync(user);
            
            // Log the password update
            var log = new Domain.Entities.UserActivityLog
            {
                UserId = user.UserId,
                ActivityType = "PasswordChanged",
                ActivityDate = DateTime.UtcNow,
                Details = $"User {user.Username} changed their password",
                IpAddress = "::1" // Local IP for demo
            };
            
            await _unitOfWork.UserActivityLogs.LogActivityAsync(log);
            
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("User {UserId} password updated successfully", user.UserId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user {UserId}: {Message}", 
                request.UserId, ex.Message);
            
            await _unitOfWork.RollbackTransactionAsync();
            
            throw;
        }
    }
} 