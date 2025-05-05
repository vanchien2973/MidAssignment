using MediatR;

namespace LibraryManagementSystem.Application.Commands.Auth
{
    public class RegisterCommand : IRequest<RegisterResponse>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
    
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
} 