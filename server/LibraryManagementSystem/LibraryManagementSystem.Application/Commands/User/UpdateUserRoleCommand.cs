using LibraryManagementSystem.Domain.Enums;
using MediatR;

namespace LibraryManagementSystem.Application.Commands.User;

public class UpdateUserRoleCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public UserType UserType { get; set; }
} 