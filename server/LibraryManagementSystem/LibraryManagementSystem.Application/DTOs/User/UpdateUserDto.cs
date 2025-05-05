using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.Application.DTOs.User
{
    public class UpdateUserDto
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public UserType? UserType { get; set; }
        public bool? IsActive { get; set; }
    }
} 