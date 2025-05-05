namespace LibraryManagementSystem.Application.DTOs.User
{
    public class UpdateUserProfileDto
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
} 