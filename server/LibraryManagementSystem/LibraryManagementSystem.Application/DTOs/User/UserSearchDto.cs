using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.User
{
    public class UserSearchDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int PageNumber { get; set; } = 1;
        
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
        
        public string? SearchTerm { get; set; } = "";
    }
} 