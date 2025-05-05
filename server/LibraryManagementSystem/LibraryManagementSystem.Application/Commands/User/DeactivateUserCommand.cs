using MediatR;

namespace LibraryManagementSystem.Application.Commands.User
{
    public class DeactivateUserCommand : IRequest<bool>
    {
        public int UserId { get; set; }

        public DeactivateUserCommand(int userId)
        {
            UserId = userId;
        }
    }
} 