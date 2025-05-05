using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.Auth
{
    public class LoginDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
} 