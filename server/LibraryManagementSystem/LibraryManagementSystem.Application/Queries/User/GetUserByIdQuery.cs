using LibraryManagementSystem.Application.DTOs.User;
using MediatR;

namespace LibraryManagementSystem.Application.Queries.User;

public class GetUserByIdQuery : IRequest<UserDto>
{
    public int UserId { get; set; }
    
    public GetUserByIdQuery(int userId)
    {
        UserId = userId;
    }
} 