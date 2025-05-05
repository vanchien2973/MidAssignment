using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.User
{
    public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivateUserCommandHandler> _logger;

        public DeactivateUserCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeactivateUserCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);

                if (!user.IsActive)
                {
                    _logger.LogInformation("User {UserId} is already inactive", request.UserId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return true;
                }

                user.IsActive = false;
                
                await _unitOfWork.Users.UpdateAsync(user);

                // Log the activity
                var activityLog = new Domain.Entities.UserActivityLog
                {
                    UserId = user.UserId,
                    ActivityType = "UserDeactivated",
                    ActivityDate = DateTime.UtcNow,
                    Details = "User account has been deactivated",
                    IpAddress = "::1" // Local IP for now
                };
                
                await _unitOfWork.UserActivityLogs.LogActivityAsync(activityLog);
                
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("User {UserId} has been deactivated", request.UserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}: {Message}", request.UserId, ex.Message);
                
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