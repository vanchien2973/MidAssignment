using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.Application.DTOs.User;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public UserType UserType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
} 