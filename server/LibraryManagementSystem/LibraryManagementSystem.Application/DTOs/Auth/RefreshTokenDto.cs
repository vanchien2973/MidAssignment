using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.Auth
{
    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
} 