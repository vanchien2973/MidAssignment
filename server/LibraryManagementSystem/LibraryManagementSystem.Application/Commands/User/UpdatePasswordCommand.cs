using MediatR;

namespace LibraryManagementSystem.Application.Commands.User;

public class UpdatePasswordCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
} 