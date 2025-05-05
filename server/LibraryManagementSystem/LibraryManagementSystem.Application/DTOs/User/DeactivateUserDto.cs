using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.User
{
    public class DeactivateUserDto
    {
        [Required]
        public int UserId { get; set; }
    }
} 