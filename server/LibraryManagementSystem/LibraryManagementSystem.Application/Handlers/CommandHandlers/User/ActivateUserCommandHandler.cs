using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.User
{
    public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ActivateUserCommandHandler> _logger;

        public ActivateUserCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ActivateUserCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                
                if (user.IsActive)
                {
                    _logger.LogInformation("User {UserId} is already active", request.UserId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return true;
                }

                user.IsActive = true;
                
                await _unitOfWork.Users.UpdateAsync(user);

                // Log the activity
                var activityLog = new Domain.Entities.UserActivityLog
                {
                    UserId = user.UserId,
                    ActivityType = "UserActivated",
                    ActivityDate = DateTime.UtcNow,
                    Details = "User account has been activated",
                    IpAddress = "::1" // Local IP for now
                };
                
                await _unitOfWork.UserActivityLogs.LogActivityAsync(activityLog);
                
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("User {UserId} has been activated", request.UserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}: {Message}", request.UserId, ex.Message);
                
                try
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error rolling back transaction");
                }
                
                return false;
            }
        }
    }
} 