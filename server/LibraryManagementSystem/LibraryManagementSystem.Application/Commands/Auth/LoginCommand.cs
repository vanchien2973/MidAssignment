using MediatR;

namespace LibraryManagementSystem.Application.Commands.Auth
{
    public class LoginCommand : IRequest<LoginResponse>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public Domain.Entities.User User { get; set; }
    }
} 