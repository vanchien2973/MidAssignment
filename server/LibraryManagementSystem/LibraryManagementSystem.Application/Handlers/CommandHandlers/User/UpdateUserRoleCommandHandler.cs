using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.User;

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserRoleCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    
    public UpdateUserRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserRoleCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }
    
    public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {   
            await _unitOfWork.BeginTransactionAsync();
            
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found during role update attempt", request.UserId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }

            // Prevent users from changing their own role
            if (request.UserId == _currentUserService.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to change their own role", _currentUserService.UserId);
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException("You cannot change your own role");
            }

            // Update role
            user.UserType = request.UserType;
            
            await _unitOfWork.Users.UpdateAsync(user);
            
            // Log the user role update
            var log = new Domain.Entities.UserActivityLog
            {
                UserId = _currentUserService.UserId,
                ActivityType = "UserRoleUpdated",
                ActivityDate = DateTime.UtcNow,
                Details = $"User {user.Username} role updated to {user.UserType} by admin",
                IpAddress = "::1" // Local IP for demo
            };
            
            await _unitOfWork.UserActivityLogs.LogActivityAsync(log);
            
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("User {UserId} role updated successfully", user.UserId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId} role: {Message}", 
                request.UserId, ex.Message);
            
            await _unitOfWork.RollbackTransactionAsync();
            
            throw;
        }
    }
} 