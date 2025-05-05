using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.User
{
    public class UpdatePasswordDto
    {
        public int UserId { get; set; }
        
        [Required]
        public string CurrentPassword { get; set; }
        
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; }
        
        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }
    }
} 