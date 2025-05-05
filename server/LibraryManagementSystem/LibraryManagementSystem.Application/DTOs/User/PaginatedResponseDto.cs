namespace LibraryManagementSystem.Application.DTOs.User
{
    public class PaginatedResponseDto<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Data retrieved successfully";
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
} 