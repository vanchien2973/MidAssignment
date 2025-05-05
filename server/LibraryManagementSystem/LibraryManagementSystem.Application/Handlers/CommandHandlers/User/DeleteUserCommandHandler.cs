using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.User;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);

            // Hard delete - xóa vĩnh viễn user khỏi database
            await _unitOfWork.Users.DeleteAsync(user.UserId);

            // Log the deletion for audit purposes
            var log = new Domain.Entities.UserActivityLog
            {
                UserId = _currentUserService.UserId,
                ActivityType = "UserDeleted",
                ActivityDate = DateTime.UtcNow,
                Details = $"User {user.Username} permanently deleted by admin",
                IpAddress = "::1" // Local IP for demo
            };
            
            await _unitOfWork.UserActivityLogs.LogActivityAsync(log);
            
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("User {UserId} permanently deleted", request.UserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}: {Message}", request.UserId, ex.Message);
            
            await _unitOfWork.RollbackTransactionAsync();
            
            throw;
        }
    }
} 