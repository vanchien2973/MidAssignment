namespace LibraryManagementSystem.Application.DTOs.Auth
{
    public class CurrentUserDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserData Data { get; set; }

        public class UserData
        {
            public string UserId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public string UserType { get; set; }
        }
    }
} 