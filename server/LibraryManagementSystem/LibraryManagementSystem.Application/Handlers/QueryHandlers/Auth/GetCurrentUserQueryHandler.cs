using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.Auth
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCurrentUserQueryHandler> _logger;

        public GetCurrentUserQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCurrentUserQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId) || !int.TryParse(request.UserId, out int userId))
                {
                    return new CurrentUserResponse
                    {
                        Success = false
                    };
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", request.UserId);
                    return new CurrentUserResponse
                    {
                        Success = false
                    };
                }

                return user.ToResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", request.UserId);
                return new CurrentUserResponse
                {
                    Success = false
                };
            }
        }
    }
} 