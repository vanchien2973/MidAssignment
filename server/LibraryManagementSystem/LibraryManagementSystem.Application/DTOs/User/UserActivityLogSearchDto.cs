using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.User
{
    public class UserActivityLogSearchDto
    {
        [Required]
        public int UserId { get; set; }
        
        public string ActivityType { get; set; } = "";
        
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int PageNumber { get; set; } = 1;
        
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
    }
} 