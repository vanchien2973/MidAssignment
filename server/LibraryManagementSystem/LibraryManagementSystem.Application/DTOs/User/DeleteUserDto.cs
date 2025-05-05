using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.User
{
    public class DeleteUserDto
    {
        [Required]
        public int UserId { get; set; }
    }
} 