using LibraryManagementSystem.Application.DTOs.Auth;
using MediatR;

namespace LibraryManagementSystem.Application.Queries.Auth
{
    public class GetCurrentUserQuery : IRequest<CurrentUserResponse>
    {
        public string UserId { get; set; }
    }

    public class CurrentUserResponse
    {
        public bool Success { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; }
    }
} 