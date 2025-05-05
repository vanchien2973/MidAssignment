using System.Collections.Generic;

namespace LibraryManagementSystem.Application.DTOs.User
{
    public class UsersResponseDto
    {
        public bool success { get; set; }
        public string message { get; set; }
        public IEnumerable<UserDto> data { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int totalCount { get; set; }
        public int totalPages { get; set; }
    }
} 