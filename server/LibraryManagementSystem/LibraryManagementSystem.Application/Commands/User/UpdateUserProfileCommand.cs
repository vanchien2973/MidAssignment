using MediatR;

namespace LibraryManagementSystem.Application.Commands.User;

public class UpdateUserProfileCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
} 