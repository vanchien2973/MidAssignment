using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.Application.DTOs.User
{
    public class UpdateUserRoleDto
    {
        public int UserId { get; set; }
        public UserType UserType { get; set; }
    }
} 