using MediatR;

namespace LibraryManagementSystem.Application.Commands.User;

public class DeleteUserCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public DeleteUserCommand(int userId)
    {
        UserId = userId;
    }
} 