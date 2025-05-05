using MediatR;

namespace LibraryManagementSystem.Application.Commands.Auth
{
    public class RefreshTokenCommand : IRequest<RefreshTokenResponse>
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
    
    public class RefreshTokenResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
} 