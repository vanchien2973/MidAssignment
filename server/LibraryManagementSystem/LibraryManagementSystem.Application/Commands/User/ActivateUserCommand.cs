using MediatR;

namespace LibraryManagementSystem.Application.Commands.User
{
    public class ActivateUserCommand : IRequest<bool>
    {
        public int UserId { get; set; }

        public ActivateUserCommand(int userId)
        {
            UserId = userId;
        }
    }
} 