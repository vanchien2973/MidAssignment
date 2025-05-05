using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.User;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    
    public UpdateUserProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserProfileCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }
    
    public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {   
            await _unitOfWork.BeginTransactionAsync();
            
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found during profile update attempt", request.UserId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
            
            // Update user information
            if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email)
            {
                // Check if email is already in use
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                if (existingUser != null && existingUser.UserId != request.UserId)
                {
                    _logger.LogWarning("Email {Email} is already in use by another user", request.Email);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }
                
                user.Email = request.Email;
            }
            
            if (!string.IsNullOrEmpty(request.FullName))
            {
                user.FullName = request.FullName;
            }
            
            await _unitOfWork.Users.UpdateAsync(user);
            
            // Log the profile update
            var log = new Domain.Entities.UserActivityLog
            {
                UserId = _currentUserService.UserId,
                ActivityType = "ProfileUpdated",
                ActivityDate = DateTime.UtcNow,
                Details = $"User {user.Username} updated their profile information",
                IpAddress = "::1" // Local IP for demo
            };
            
            await _unitOfWork.UserActivityLogs.LogActivityAsync(log);
            
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("User {UserId} profile updated successfully", user.UserId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}: {Message}", 
                request.UserId, ex.Message);
            
            await _unitOfWork.RollbackTransactionAsync();
            
            throw;
        }
    }
} 